using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAIApp.Models;
using Supabase;

namespace OpenAIApp.Repository.TagRepo
{
    public class TagRepo : ITagRepo
    {
        private readonly Client _supabaseClient;

        public TagRepo(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<Tag> CreateTagAsync(Tag tag)
        {
            var response = await _supabaseClient.From<Tag>().Insert(new List<Tag> { tag });
            return response.Models.FirstOrDefault();
        }

        public async Task<Tag> GetTagByIdAsync(Guid id)
        {
            var response = await _supabaseClient.From<Tag>().Where(x => x.Id == id).Get();
            return response.Model;
        }

        public async Task<List<Tag>> GetTagsAsync()
        {
            var response = await _supabaseClient.From<Tag>().Get();
            return response.Models;
        }

        public async Task<Tag> UpdateTagAsync(Tag updatedTag)
        {
            var response = await _supabaseClient.From<Tag>().Update(updatedTag);
            return response.Models.FirstOrDefault();
        }

        public async Task DeleteTagAsync(Guid id)
        {
            await _supabaseClient.From<Tag>().Delete();
        }
    }
}
