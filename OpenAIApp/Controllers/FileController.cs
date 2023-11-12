using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenAIApp.Models;
using OpenAIApp.Repository.FileRepo;

namespace OpenAIApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly IFileRepo _fileRepo;

        public FileController(ILogger<FileController> logger, IFileRepo fileRepo)
        {
            _logger = logger;
            _fileRepo = fileRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFiles()
        {
            try
            {
                var files = await _fileRepo.GetFilesAsync();
                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(Guid id)
        {
            try
            {
                var file = await _fileRepo.GetFileByIdAsync(id);
                if (file == null)
                    return NotFound();
                return Ok(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting file with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFile([FromBody] FileModel file)
        {
            try
            {
                var createdFile = await _fileRepo.CreateFileAsync(file);
                return CreatedAtAction(nameof(GetFile), new { id = createdFile.Id }, createdFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating file");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFile(Guid id, [FromBody] FileModel file)
        {
            try
            {
                var updatedFile = await _fileRepo.UpdateFileAsync(file);
                if (updatedFile == null)
                    return NotFound();
                return Ok(updatedFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating file with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(Guid id)
        {
            try
            {
                await _fileRepo.DeleteFileAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // Add other endpoints as necessary
    }
}
