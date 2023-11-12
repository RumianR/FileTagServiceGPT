using OpenAIApp.Enums;
using OpenAIApp.Models;
using Supabase;
using static Postgrest.Constants;

namespace OpenAIApp.Repository.FileRepo
{
    public class FileRepo : IFileRepo
    {
        private readonly Client _supabaseClient;

        public FileRepo(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<FileModel> CreateFileAsync(FileModel file)
        {
            var response = await _supabaseClient
                .From<FileModel>()
                .Insert(new List<FileModel> { file });

            return response.Models.FirstOrDefault();
        }

        public async Task<FileModel> GetFileByIdAsync(Guid id)
        {
            var response = await _supabaseClient.From<FileModel>().Where(x => x.Id == id).Get();

            return response.Model;
        }

        public async Task<List<FileModel>> GetFilesAsync()
        {
            var response = await _supabaseClient.From<FileModel>().Get();

            return response.Models;
        }

        public async Task<FileModel> UpdateFileAsync(FileModel updatedFile)
        {
            var response = await _supabaseClient.From<FileModel>().Update(updatedFile);

            return response.Models.FirstOrDefault();
        }

        public async Task DeleteFileAsync(Guid id)
        {
            await _supabaseClient.From<FileModel>().Delete();
        }

        public async Task<List<FileModel>> GetFilesByStates(List<FileState> states)
        {
            var statesIntFormat = states.Select(state => (object)(int)state);
            var response = await _supabaseClient
                .From<FileModel>()
                .Filter(x => x.State, Operator.In, statesIntFormat.ToList())
                //.Order(x => x.CreatedAt, Ordering.Ascending)
                .Get();
            return response.Models;
        }
    }
}
