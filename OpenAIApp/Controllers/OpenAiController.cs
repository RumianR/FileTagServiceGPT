using Microsoft.AspNetCore.Mvc;
using OpenAIApp.Helpers.OpenAi;
using OpenAIApp.Models;
using OpenAIApp.Services.FileProcessing;
using Supabase;
using FileModel = OpenAIApp.Models.FileModel;

namespace OpenAIApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OpenAiController : ControllerBase
    {
        private readonly ILogger<OpenAiController> _logger;
        private readonly IOpenAiHelper _openAiService;
        private readonly Client _supabaseClient;
        private readonly IFileProcesssingService _fileProcessingService;

        public OpenAiController(
            ILogger<OpenAiController> logger,
            IOpenAiHelper openAiService,
            Client supabaseClient,
            IFileProcesssingService fileProcessingService
        )
        {
            _logger = logger;
            _openAiService = openAiService;
            _supabaseClient = supabaseClient;
            _fileProcessingService = fileProcessingService;
        }

        [HttpGet()]
        [Route("CompleteSentence")]
        public async Task<IActionResult> GetAllFiles()
        {
            // Get All Messages
            var response = await _supabaseClient.Postgrest.Table<FileModel>().Get();
            List<FileModel> models = response.Models;
            return Ok(models);
        }

        [HttpPost()]
        [Route("ProcessNewFile")]
        public async Task<IActionResult> ProcessNewFile(string text)
        {
            _fileProcessingService.AddNewFileToQueue(text);
            return Ok();
        }

        [HttpPost()]
        [Route("CompleteSentence")]
        public async Task<IActionResult> CompleteSentence(string text)
        {
            var result = await _openAiService.CompleteSentence(text);
            return Ok(result);
        }

        [HttpPost()]
        [Route("CompleteSentenceAdvanced")]
        public async Task<IActionResult> CompleteSentenceAdvanced(string text)
        {
            var result = await _openAiService.CompleteSentenceAdvanced(text);
            return Ok(result);
        }
    }
}
