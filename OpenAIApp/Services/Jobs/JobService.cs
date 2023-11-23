using OpenAIApp.Configurations;
using OpenAIApp.Enums;
using OpenAIApp.Models;
using OpenAIApp.Repository.FileRepo;
using OpenAIApp.Services.FileProcessing;

namespace OpenAIApp.Services.Jobs
{
    public class JobService : IService
    {
        private readonly IFileRepo _fileRepo;
        private readonly IFileProcesssingService _fileProcessingService;
        private readonly Timer _timer;
        private readonly int _intervalInSeconds = PollingConfig.JobServicePollingIntervalInSeconds;
        private readonly ILogger<JobService> _logger;

        public JobService(IFileRepo fileRepo, IFileProcesssingService fileProcessingService, ILogger<JobService> logger)
        {
            _fileRepo = fileRepo;
            _fileProcessingService = fileProcessingService;
            _timer = new Timer(PollForFiles);
            _logger = logger;
        }

        public void Start()
        {
            _timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private async void PollForFiles(object? state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            var filesToProcess = new List<FileModel>();

            try
            {
                filesToProcess = await _fileRepo.GetFilesByStates(new List<FileState> { PollingConfig.PollingState });
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Could not poll for jobs: {ex.Message}");
            }

            if (filesToProcess.Any())
            {
                _logger.LogDebug($"Polling returned {filesToProcess.Count} uploaded items");
            }

            foreach (var file in filesToProcess)
            {
                _fileProcessingService.AddNewFileToQueue(file.Id.ToString());
            }

            _timer.Change(
                TimeSpan.FromSeconds(_intervalInSeconds),
                TimeSpan.FromSeconds(_intervalInSeconds)
            );
        }
    }
}
