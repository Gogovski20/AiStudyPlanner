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

            var userId = GetCurrentUserId();
            
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _aiService.GetStudyPlanAsync(request.Input);

                var chat = new ChatHistory
                {
                    UserId = userId.Value,
                    UserInput = request.Input,
                    Tasks = result.Tasks,
                    EstimatedTime = result.EstimatedTime,
                    Priority = result.Priority,
                    CreatedAt = DateTime.UtcNow
                };

                await _chatHistoryRepository.AddAsync(chat);
                await _chatHistoryRepository.SaveChangesAsync();

                return Ok(new
                {
                    chat.Id,
                    result.Tasks,
                    result.EstimatedTime,
                    result.Priority,
                    Progress = CalculateProgress(result.Tasks)
                });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("AI service is busy"))
                    return StatusCode(503, new { message = ex.Message});

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

            var history = await _chatHistoryRepository.GetByIdAsync(id);
            
            if (history == null)
                return NotFound(new { message = "Chat history not found." });

            if (history.UserId != userId.Value)
                return Forbid();

            return Ok(new
            {
                history.Id,
                history.UserInput,
                history.Tasks,
                history.EstimatedTime,
                history.Priority,
                history.CreatedAt,
                Progress = CalculateProgress(history.Tasks)
            });
        }

        [HttpPatch("history/{historyId:int}/tasks/{taskIndex:int}/complete")]
        public async Task<IActionResult> CompleteTask(int historyId, int taskIndex)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var chatHistory = await _chatHistoryRepository.GetByIdAsync(historyId);

            if (chatHistory == null)
                return NotFound(new { message = "Chat history not found." });

            if (chatHistory.UserId != userId.Value)
                return Forbid();

            if (taskIndex < 0 || taskIndex >= chatHistory.Tasks.Count)
                return BadRequest(new { message = "Invalid task index." });

            chatHistory.Tasks[taskIndex].IsCompleted = true;

            await _chatHistoryRepository.SaveChangesAsync();

            return Ok(new
            {
                message = "Task marked as completed.",
                chatHistory.Id,
                updatedTask = chatHistory.Tasks[taskIndex],
                progress = CalculateProgress(chatHistory.Tasks)
            });
        }

        [HttpPatch("history/{historyId:int}/tasks/{taskIndex:int}/incomplete")]
        public async Task<IActionResult> IncompleteTask(int historyId, int taskIndex)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var chatHistory = await _chatHistoryRepository.GetByIdAsync(historyId);

            if (chatHistory == null)
                return NotFound(new { message = "Chat history not found." });

            if (chatHistory.UserId != userId.Value)
                return Forbid();

            if (taskIndex < 0 || taskIndex >= chatHistory.Tasks.Count)
                return BadRequest(new { message = "Invalid task index." });

            chatHistory.Tasks[taskIndex].IsCompleted = false;

            await _chatHistoryRepository.SaveChangesAsync();

            return Ok(new
            {
                message = "Task marked as incomplete.",
                chatHistory.Id,
                updatedTask = chatHistory.Tasks[taskIndex],
                progress = CalculateProgress(chatHistory.Tasks)
            });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var history = await _chatHistoryRepository.GetByUserIdAsync(userId.Value);

            return Ok(history);
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

        private static object CalculateProgress(List<TaskItem> tasks)
        {
            if (tasks.Count == 0)
            {
                return new
                {
                    completedTasks = 0,
                    totalTasks = 0,
                    percentage = 0
                };
            }

            int completed = tasks.Count(t => t.IsCompleted);
            int total = tasks.Count;
            double percentage = Math.Round((double)completed / total * 100, 2);

            return new
            {
                completedTasks = completed,
                totalTasks = total,
                percentage
            };
        }
    }
}