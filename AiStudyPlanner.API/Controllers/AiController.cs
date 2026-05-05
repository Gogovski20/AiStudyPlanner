using AiStudyPlanner.API.Contracts.Ai;
using AiStudyPlanner.API.Mappers;
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
        private readonly IStudyPlanService _studyPlanService;

        public AiController(IStudyPlanService studyPlanService)
        {
            _studyPlanService = studyPlanService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] AiRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Input))
                return BadRequest("Input cannot be empty.");

            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            try
            {
                var chat = await _studyPlanService.GenerateAndSaveAsync(userId.Value, request.Input);

                return Ok(AiResponseMapper.ToGenerateStudyPlanResponse(chat));
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("AI service is busy"))
                    return StatusCode(503, new { message = ex.Message });

                return StatusCode(500, new
                {
                    message = "Something went wrong while generating the study plan."
                });
            }
        }

        [HttpGet("history/{id:int}")]
        public async Task<IActionResult> GetHistoryById(int id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var history = await _studyPlanService.GetHistoryByIdAsync(userId.Value, id);

            if (history == null)
                return NotFound(new { message = "Chat history not found." });

            return Ok(AiResponseMapper.ToChatHistoryResponse(history));
        }

        [HttpPatch("history/{historyId:int}/tasks/{taskIndex:int}/complete")]
        public async Task<IActionResult> CompleteTask(int historyId, int taskIndex)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            try
            {
                var chatHistory = await _studyPlanService.CompleteTaskAsync(
                    userId.Value,
                    historyId,
                    taskIndex
                );

                if (chatHistory == null)
                    return NotFound(new { message = "Chat history not found." });

                return Ok(new
                {
                    message = "Task marked as completed",
                    chatHistory.Id,
                    updatedTask = AiResponseMapper.ToTaskItemResponse(chatHistory.Tasks[taskIndex]),
                    progress = AiResponseMapper.CalculateProgress(chatHistory.Tasks)
                });
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest(new { message = "Invalid task index." });
            }
        }

        [HttpPatch("history/{historyId:int}/tasks/{taskIndex:int}/incomplete")]
        public async Task<IActionResult> IncompleteTask(int historyId, int taskIndex)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            try
            {
                var chatHistory = await _studyPlanService.IncompleteTaskAsync(
                    userId.Value,
                    historyId,
                    taskIndex
                );

                if (chatHistory == null)
                    return NotFound(new { message = "Chat history not found." });

                return Ok(new
                {
                    message = "Task marked as incomplete.",
                    chatHistory.Id,
                    updatedTask = AiResponseMapper.ToTaskItemResponse(chatHistory.Tasks[taskIndex]),
                    progress = AiResponseMapper.CalculateProgress(chatHistory.Tasks)
                });
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest(new { message = "Invalid task index." });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var history = await _studyPlanService.GetHistoryAsync(userId.Value);

            var response = history.Select(AiResponseMapper.ToChatHistoryResponse).ToList();

            return Ok(response);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                return null;

            if (!int.TryParse(userIdClaim, out int userId))
                return null;

            return userId;
        }
    }
}