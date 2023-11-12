using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAIApp.Models;

namespace OpenAIApp.Repository.TagRepo
{
    public interface ITagRepo
    {
        Task<Tag> CreateTagAsync(Tag tag);
        Task<Tag> GetTagByIdAsync(Guid id);
        Task<List<Tag>> GetTagsAsync();
        Task<Tag> UpdateTagAsync(Tag updatedTag);
        Task DeleteTagAsync(Guid id);
    }
}
