using AiStudyPlanner.Application.Repositories;
using AiStudyPlanner.Application.Services.Interfaces;
using AiStudyPlanner.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AiStudyPlanner.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly IChatHistoryRepository _chatHistoryRepository;

        public AiController(
            IAiService aiService,
            IChatHistoryRepository chatHistoryRepository)
        {
            _aiService = aiService;
            _chatHistoryRepository = chatHistoryRepository;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] AiRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Input))
                return BadRequest("Input cannot be empty.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            try
            {
                var result = await _aiService.GetStudyPlanAsync(request.Input);

                var chat = new ChatHistory
                {
                    UserId = userId,
                    UserInput = request.Input,
                    Tasks = result.Tasks,
                    EstimatedTime = result.EstimatedTime,
                    Priority = result.Priority,
                    CreatedAt = DateTime.UtcNow
                };

                await _chatHistoryRepository.AddAsync(chat);
                await _chatHistoryRepository.SaveChangesAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("AI service is busy"))
                    return StatusCode(503, ex.Message);

                return StatusCode(500, "Something went wrong while generating the study plan.");
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var history = await _chatHistoryRepository.GetByUserIdAsync(userId);

            return Ok(history);
        }
    }
}