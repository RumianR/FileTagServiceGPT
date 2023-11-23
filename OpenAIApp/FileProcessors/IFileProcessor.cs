using OpenAIApp.Common;

namespace OpenAIApp.FileProcessors
{
    public interface IFileProcessor
    {
        Task<FileMetadata> GetFileMetadataAsync(string url, Guid fileId, int maxPages = int.MaxValue);
    }
}
