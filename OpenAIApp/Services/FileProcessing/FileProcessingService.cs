using Newtonsoft.Json;
using OpenAIApp.Configurations;
using OpenAIApp.Enums;
using OpenAIApp.Helpers.Files;
using OpenAIApp.Helpers.OpenAi;
using OpenAIApp.Models;
using OpenAIApp.Models.OpenAi;
using OpenAIApp.Repository.FileRepo;
using OpenAIApp.Repository.FileTagRepo;
using OpenAIApp.Repository.TagRepo;
using Supabase;

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

        public FileProcessingService(
            ILogger<FileProcessingService> logger,
            Client supabaseClient,
            IOpenAiHelper openAiHelper,
            IFileRepo fileRepo,
            ITagRepo tagRepo,
            IFileTagRepo fileTagRepo
        )
        {
            _logger = logger;
            _fileProcessingQueue = new Queue<string>();
            _supabaseClient = supabaseClient;
            _openAiHelper = openAiHelper;
            _fileRepo = fileRepo;
            _tagRepo = tagRepo;
            _fileTagRepo = fileTagRepo;

            _timer = new Timer(OnTimerProcessFiles);
        }

        public async void AddNewFileToQueue(string fileId)
        {
            var file = await _fileRepo.GetFileByIdAsync(Guid.Parse(fileId));

            if (file != null && file.State == (int)FileState.UPLOADED)
            {
                _fileProcessingQueue.Enqueue(fileId);

                await UpdateState(file, FileState.QUEUED);
            }
        }

        private async void OnTimerProcessFiles(object? state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            var tasks = new List<Task>();
            while (_fileProcessingQueue.Count > 0 && tasks.Count < 5)
            {
                var fileId = _fileProcessingQueue.Dequeue();
                tasks.Add(HandleFileCatchAll(fileId));
            }

            await Task.WhenAll(tasks);

            _timer.Change(
                TimeSpan.FromSeconds(_intervalInSeconds),
                TimeSpan.FromSeconds(_intervalInSeconds)
            );
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

            if (!file.MimeType.Contains("pdf"))
            {
                _logger.LogDebug($"We are only processing pdf files for the moment");
                return;
            }

            await UpdateState(file, FileState.PROCESSING);

            var metadata = await PdfHelper.GetFileMetadataAsync(file.Url);

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
                    existingTag = await _tagRepo.CreateTagAsync(tag);
                }



                await _fileTagRepo.CreateFileTagAsync(
                    new FileTag { FileId = file.Id, TagId = existingTag.Id }
                );
            });


            await UpdateState(file, FileState.COMPLETED);
        }

        private async Task UpdateState(FileModel file, FileState fileState)
        {
            file.State = (int)fileState;
            await _fileRepo.UpdateFileAsync(file);
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
