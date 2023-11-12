namespace OpenAIApp.Services.FileProcessing
{
    public interface IFileProcesssingService : IService
    {
        public void AddNewFileToQueue(string fileId);
    }
}
