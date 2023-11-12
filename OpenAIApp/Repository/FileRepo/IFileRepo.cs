using OpenAIApp.Enums;
using OpenAIApp.Models;

namespace OpenAIApp.Repository.FileRepo
{
    public interface IFileRepo
    {
        Task<List<FileModel>> GetFilesByStates(List<FileState> states);
        Task<FileModel> CreateFileAsync(FileModel file);
        Task<List<FileModel>> GetFilesAsync();
        Task<FileModel> UpdateFileAsync(FileModel updatedFile);
        Task DeleteFileAsync(Guid id);
        Task<FileModel> GetFileByIdAsync(Guid id);
    }
}
