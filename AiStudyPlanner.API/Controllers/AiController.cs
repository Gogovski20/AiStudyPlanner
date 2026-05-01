using AiStudyPlanner.Application.Services.Interfaces;
using AiStudyPlanner.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace AiStudyPlanner.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] AiRequest request)
        {
            try
            {
                var result = await _aiService.GetStudyPlanAsync(request.Input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // You'll see the real message in Swagger's response body now
                return StatusCode(500, ex.Message);
            }
        }
    }
}
