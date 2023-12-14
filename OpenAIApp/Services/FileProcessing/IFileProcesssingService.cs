namespace OpenAIApp.Services.FileProcessing
{
    public interface IFileProcesssingService : IService
    {
        public Task AddNewFileToQueue(string fileId);
    }
}
