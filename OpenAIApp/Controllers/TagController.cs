using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenAIApp.Models;
using OpenAIApp.Repository.TagRepo;

namespace OpenAIApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TagController : ControllerBase
    {
        private readonly ILogger<TagController> _logger;
        private readonly ITagRepo _tagRepo;

        public TagController(ILogger<TagController> logger, ITagRepo tagRepo)
        {
            _logger = logger;
            _tagRepo = tagRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTags()
        {
            try
            {
                var tags = await _tagRepo.GetTagsAsync();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTag(Guid id)
        {
            try
            {
                var tag = await _tagRepo.GetTagByIdAsync(id);
                if (tag == null)
                    return NotFound();
                return Ok(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tag with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTag([FromBody] Tag tag)
        {
            try
            {
                var createdTag = await _tagRepo.CreateTagAsync(tag);
                return CreatedAtAction(nameof(GetTag), new { id = createdTag.Id }, createdTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTag(Guid id, [FromBody] Tag tag)
        {
            try
            {
                var updatedTag = await _tagRepo.UpdateTagAsync(tag);
                if (updatedTag == null)
                    return NotFound();
                return Ok(updatedTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating tag with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(Guid id)
        {
            try
            {
                await _tagRepo.DeleteTagAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting tag with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
