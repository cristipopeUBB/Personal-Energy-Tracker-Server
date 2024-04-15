using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
using OpenAI_API.Completions;

namespace PEC_TrackerAdvisorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OpenAiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("get-data")]
        public async Task<IActionResult> GetDataAsync(string input)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            string response = string.Empty;
            OpenAIAPI openAI = new OpenAIAPI(apiKey);
            CompletionRequest completionRequest = new()
            {
                Model = "gpt-3.5-turbo-instruct",
                MaxTokens = 4000,
                Prompt = input
            };

            var output = await openAI.Completions.CreateCompletionAsync(completionRequest);
            if(output != null)
            {
                foreach(var item in output.Completions)
                {
                    response = item.Text;
                }

                return Ok(response);
            }

            return BadRequest("Failed to get response from OpenAI API");
        }
    }
}
