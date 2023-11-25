using Newtonsoft.Json;
using OpenAIApp.Common;
using OpenAIApp.Configurations;
using OpenAIApp.Enums;
using OpenAIApp.Helpers.OpenAi;
using OpenAIApp.Models;
using OpenAIApp.Models.OpenAi;
using OpenAIApp.Repository.FileRepo;
using OpenAIApp.Repository.FileTagRepo;
using OpenAIApp.Repository.TagRepo;
using Supabase;
using System.Security.Policy;
using System;
using OpenAIApp.Helpers;
using OpenAIApp.FileProcessors;
using OpenAIApp.FileProcessors.ImageProcessing;

namespace OpenAIApp.Services.FileProcessing
{
    public class FileProcessingService : IFileProcesssingService
    {
        private readonly ILogger<FileProcessingService> _logger;
        private readonly IFileRepo _fileRepo;
        private readonly ITagRepo _tagRepo;
        private readonly IFileTagRepo _fileTagRepo;
        private Queue<string> _fileProcessingQueue;
        private Client _supabaseClient;
        private IOpenAiHelper _openAiHelper;
        private Timer _timer;
        private readonly int _intervalInSeconds =
            PollingConfig.FileProcessingServicePollingIntervalInSeconds;
        private readonly string _baseUrl = "https://nxoavkcgtuzdxbfamjjh.supabase.co/storage/v1/object/public/";

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10, 10); // Allows 5 concurrent tasks

        private readonly PdfFileProcessor _pdfFileProcessor;
        private readonly ImageFileProcessor _imageFileProcessor;


        public FileProcessingService(
            ILogger<FileProcessingService> logger,
            Client supabaseClient,
            PdfFileProcessor pdfFileProcessor,
            ImageFileProcessor imageFileProcessor,
            IOpenAiHelper openAiHelper,
            IFileRepo fileRepo,
            ITagRepo tagRepo,
            IFileTagRepo fileTagRepo
        )
        {
            _logger = logger;
            _fileProcessingQueue = new Queue<string>();
            _supabaseClient = supabaseClient;
            _pdfFileProcessor = pdfFileProcessor;
            _imageFileProcessor = imageFileProcessor;
            _openAiHelper = openAiHelper;
            _fileRepo = fileRepo;
            _tagRepo = tagRepo;
            _fileTagRepo = fileTagRepo;

            _timer = new Timer(OnTimerProcessFiles);
        }

        public async void AddNewFileToQueue(string fileId)
        {
            FileModel file = null;

            try
            {
                file = await _fileRepo.GetFileByIdAsync(Guid.Parse(fileId));
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not get file {fileId} due to : {ex.Message}");
            }

            if (file != null && file.State == (int)PollingConfig.PollingState)
            {
                _fileProcessingQueue.Enqueue(fileId);

                await UpdateState(file, FileState.QUEUED);
            }
        }

        private async void OnTimerProcessFiles(object? state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            while (_fileProcessingQueue.Count > 0)
            {
                await _semaphore.WaitAsync(); // Wait to enter the semaphore

                if (_fileProcessingQueue.TryDequeue(out var fileId))
                {
                    _ = HandleFileCatchAll(fileId).ContinueWith(t =>
                    {
                        _semaphore.Release(); // Release the semaphore when done
                    });
                }
            }

            _timer.Change(TimeSpan.FromSeconds(_intervalInSeconds), TimeSpan.FromSeconds(_intervalInSeconds));
        }

        private async Task HandleFileCatchAll(string fileId)
        {
            try
            {
                await ProcessFile(fileId);
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Could not process file: {fileId}" +
                    $"{Environment.NewLine}" +
                    $"{e.Message}", e);

                var file = await _fileRepo.GetFileByIdAsync(Guid.Parse(fileId));

                if (file != null)
                {
                    await UpdateState(file, FileState.FAILED);
                }
            }
        }

        private async Task ProcessFile(string fileId)
        {
            _logger.LogDebug($"Processing file: {fileId}");

            var file = await _fileRepo.GetFileByIdAsync(Guid.Parse(fileId));

            if (file == null)
            {
                _logger.LogDebug($"Could not process file: {fileId} because it was not found.");
                return;
            }

            var fileType = GetFileType(file);

            if (fileType == FileType.UNSUPPORTED)
            {
                await UpdateState(file, FileState.FAILED);
                return;
            }

            await UpdateState(file, FileState.PROCESSING);


            FileMetadata metadata = null;

            if (fileType == FileType.PDF)
            {
                metadata = await _pdfFileProcessor.GetFileMetadataAsync(file.Url, file.Id);
            }

            else if (fileType == FileType.IMAGE)
            {
                metadata = await _imageFileProcessor.GetFileMetadataAsync(file.Url, file.Id);
            }

            if (metadata == null)
            {
                _logger.LogDebug($"Could not process file: {fileId} because file metadata creation returned null.");
                await UpdateState(file, FileState.FAILED);
                return;
            }

            file = await UpdateThumbnail(file, metadata);
            file.Pages = metadata.NumberOfPages;
            file.Size = metadata.FileLengthInBytes;

            var fileText = metadata.FileContentText;

            if (string.IsNullOrWhiteSpace(fileText))
            {
                _logger.LogDebug(
                    $"Could not process file: {fileId} because pdf to text conversion failed or empty pdf."
                );
                await UpdateState(file, FileState.FAILED);
                return;
            }

            _logger.LogDebug($"Sending OpenAi request for file: {fileId}");
            var tagsJson = await _openAiHelper.GetTags(fileText);

            _logger.LogDebug(
                $"OpenAi response for file: {fileId}" + $"{Environment.NewLine}" + $"{tagsJson}"
            );

            TagModelOpenAi tagModelOpenAi = null;

            try
            {
                tagModelOpenAi = JsonConvert.DeserializeObject<TagModelOpenAi>(tagsJson);
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not deserialize OpenAi response: {ex.Message}", ex);
                await UpdateState(file, FileState.FAILED);
                return;
            }

            if (tagModelOpenAi == null || tagModelOpenAi.Tags.Count == 0)
            {
                _logger.LogDebug($"Could not get any tags");
                await UpdateState(file, FileState.FAILED);
                return;
            }

            var tagModels = tagModelOpenAi.Tags.Select(tag => new Tag { Name = tag }).ToList();


            tagModels.ForEach(async tag =>
            {
                var existingTag = await _tagRepo.GetTagByNameAsync(tag.Name);

                if (existingTag == null)
                {
                    try
                    {
                        existingTag = await _tagRepo.CreateTagAsync(tag);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug($"Could not create tag error: {ex.Message}");
                    }
                }

                try
                {
                    await _fileTagRepo.CreateFileTagAsync(new FileTag { FileId = file.Id, TagId = existingTag.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogDebug($"Could not create file tag error: {ex.Message}");
                }
            });

            file.Name = tagModelOpenAi.Name;
            await UpdateState(file, FileState.COMPLETED);
        }

        private FileType GetFileType(FileModel file)
        {
            var baseLog = $"file {file.Id} with mime type {file.MimeType}";

            if (file.MimeType.Contains("pdf"))
            {
                _logger.LogDebug($"{baseLog} is classified as a pdf");
                return FileType.PDF;
            }

            if (file.MimeType.Contains("image"))
            {
                _logger.LogDebug($"{baseLog} is classified as a image");
                return FileType.IMAGE;
            }

            _logger.LogDebug($"{baseLog} is an unsupported type");

            return FileType.UNSUPPORTED;
        }

        private async Task<FileModel> UpdateThumbnail(FileModel file, FileMetadata fileMetadata)
        {
            var (userId, fileFolder) = UrlHelper.ParseUserIdAndFolder(file.Url);

            var supabasePath = $"{userId}/{fileFolder}/{file.Id}_thumbnail.png";

            try
            {
                var byteArray = Convert.FromBase64String(fileMetadata.ThumbnailBase64);
                var resultPath = await _supabaseClient.Storage
                  .From($"files")
                  .Upload(byteArray, supabasePath);

                file.ThumbnailUrl = _baseUrl + resultPath;

                await UpdateState(file, FileState.PROCESSING);
            }

            catch (Exception ex)
            {
                _logger.LogDebug($"Could not upload thumbnail for file: {file.Id} due to error: {ex.Message}");
            }

            return file;
        }

        private async Task UpdateState(FileModel file, FileState fileState)
        {
            file.State = (int)fileState;

            try
            {
                await _fileRepo.UpdateFileAsync(file);

            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not update file {file.Id} state to {fileState} due to: {ex}");
            }
        }

        public void Start()
        {
            _timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
