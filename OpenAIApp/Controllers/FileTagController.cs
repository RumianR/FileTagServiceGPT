using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenAIApp.Models;
using OpenAIApp.Repository.FileTagRepo;

namespace OpenAIApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileTagController : ControllerBase
    {
        private readonly ILogger<FileTagController> _logger;
        private readonly IFileTagRepo _fileTagRepo;

        public FileTagController(ILogger<FileTagController> logger, IFileTagRepo fileTagRepo)
        {
            _logger = logger;
            _fileTagRepo = fileTagRepo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFileTag([FromBody] FileTag fileTag)
        {
            try
            {
                var createdFileTag = await _fileTagRepo.CreateFileTagAsync(fileTag);
                return CreatedAtAction(
                    nameof(CreateFileTag),
                    new
                    {
                        fileId = createdFileTag.FileId,
                        tagId = createdFileTag.TagId
                    },
                    createdFileTag
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating file tag");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("file/{fileId}")]
        public async Task<IActionResult> GetFileTagsByFileId(Guid fileId)
        {
            try
            {
                var fileTags = await _fileTagRepo.GetFileTagByFileId(fileId);
                return Ok(fileTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting file tags for file with id {fileId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("tag/{tagId}")]
        public async Task<IActionResult> GetFileTagsByTagId(Guid tagId)
        {
            try
            {
                var fileTags = await _fileTagRepo.GetFileTagByTagId(tagId);
                return Ok(fileTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting file tags for tag with id {tagId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFileTags()
        {
            try
            {
                var fileTags = await _fileTagRepo.GetFileTagsAsync();
                return Ok(fileTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file tags");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFileTag(long id)
        {
            try
            {
                await _fileTagRepo.DeleteFileTagAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file tag with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
