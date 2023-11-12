using OpenAIApp.Configurations;
using OpenAIApp.Enums;
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

        public JobService(IFileRepo fileRepo, IFileProcesssingService fileProcessingService)
        {
            _fileRepo = fileRepo;
            _fileProcessingService = fileProcessingService;
            _timer = new Timer(PollForFiles);
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

            var filesToProcess = await _fileRepo.GetFilesByStates(
                new List<FileState> { FileState.UPLOADED }
            );

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
