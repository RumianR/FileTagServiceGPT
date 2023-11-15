using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAIApp.Models;
using Supabase;

namespace OpenAIApp.Repository.FileTagRepo
{
    public class FileTagRepo : IFileTagRepo
    {
        private readonly Client _supabaseClient;

        public FileTagRepo(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<List<FileTag>> GetFileTagByFileId(Guid fileId)
        {
            var response = await _supabaseClient
                .From<FileTag>()
                .Where(x => x.FileId == fileId)
                .Get();
            return response.Models;
        }

        public async Task<List<FileTag>> GetFileTagByTagId(Guid tagId)
        {
            var response = await _supabaseClient.From<FileTag>().Where(x => x.TagId == tagId).Get();
            return response.Models;
        }

        public async Task<FileTag> CreateFileTagAsync(FileTag fileTag)
        {
            var response = await _supabaseClient
                .From<FileTag>()
                .Upsert(new List<FileTag> { fileTag });
            return response.Models.FirstOrDefault();
        }

        public async Task<List<FileTag>> GetFileTagsAsync()
        {
            var response = await _supabaseClient.From<FileTag>().Get();
            return response.Models;
        }

        public async Task DeleteFileTagAsync(long id)
        {
            await _supabaseClient.From<FileTag>().Delete();
        }
    }
}
