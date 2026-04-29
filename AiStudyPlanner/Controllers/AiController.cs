using AiStudyPlanner.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace AiStudyPlanner.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        [HttpPost("generate")]
        public IActionResult Generate([FromBody] AiRequest request)
        {
            return Ok(new
            {
                message = "Backend working",
                request = request.Input
            });
        }
    }
}
