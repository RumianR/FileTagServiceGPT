using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAIApp.Models;

namespace OpenAIApp.Repository.FileTagRepo
{
    public interface IFileTagRepo
    {
        Task<FileTag> CreateFileTagAsync(FileTag fileTag);
        Task<FileTag> GetFileTagByIdAsync(long id);
        Task<List<FileTag>> GetFileTagsAsync();
        Task<List<FileTag>> GetFileTagByFileId(Guid fileId);
        Task<List<FileTag>> GetFileTagByTagId(Guid tagId);
        Task DeleteFileTagAsync(long id);
    }
}
