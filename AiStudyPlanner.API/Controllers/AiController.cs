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
        public async Task<IActionResult> Generate([FromBody] GenerateStudyPlanRequest request)
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

        [HttpPatch("history/{historyId:int}/tasks/{taskId:guid}/complete")]
        public async Task<IActionResult> CompleteTask(int historyId, Guid taskId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            try
            {
                var chatHistory = await _studyPlanService.CompleteTaskAsync(
                    userId.Value,
                    historyId,
                    taskId
                );

                if (chatHistory == null)
                    return NotFound(new { message = "Chat history not found." });

                var updatedTask = chatHistory.Tasks.First(t => t.Id == taskId);

                return Ok(new
                {
                    message = "Task marked as completed",
                    chatHistory.Id,
                    updatedTask = AiResponseMapper.ToTaskItemResponse(updatedTask),
                    progress = AiResponseMapper.CalculateProgress(chatHistory.Tasks)
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Task not found." });
            }
        }

        [HttpPatch("history/{historyId:int}/tasks/{taskId:guid}/incomplete")]
        public async Task<IActionResult> IncompleteTask(int historyId, Guid taskId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            try
            {
                var chatHistory = await _studyPlanService.IncompleteTaskAsync(
                    userId.Value,
                    historyId,
                    taskId
                );

                if (chatHistory == null)
                    return NotFound(new { message = "Chat history not found." });

                var updatedTask = chatHistory.Tasks.First(t => t.Id == taskId);

                return Ok(new
                {
                    message = "Task marked as incomplete.",
                    chatHistory.Id,
                    updatedTask = AiResponseMapper.ToTaskItemResponse(updatedTask),
                    progress = AiResponseMapper.CalculateProgress(chatHistory.Tasks)
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Task not found." });
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

        [HttpPatch("history/{historyId:int}/tasks/{taskId:guid}/title")]
        public async Task<IActionResult> UpdateTaskTitle(
            int historyId,
            Guid taskId,
            [FromBody] UpdateTaskTitleRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            try
            {
                var chatHistory = await _studyPlanService.UpdateTaskTitleAsync(
                    userId.Value,
                    historyId,
                    taskId,
                    request.Title
                );

                if (chatHistory == null)
                    return NotFound(new { message = "Chat history not found." });

                var updatedTask = chatHistory.Tasks.First(t => t.Id == taskId);

                return Ok(new
                {
                    message = "Task title updated.",
                    chatHistory.Id,
                    updatedTask = AiResponseMapper.ToTaskItemResponse(updatedTask),
                    progress = AiResponseMapper.CalculateProgress(chatHistory.Tasks)
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Task not found." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("history/{historyId:int}/tasks/{taskId:guid}")]
        public async Task<IActionResult> DeleteTask(int historyId, Guid taskId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            try
            {
                var chatHistory = await _studyPlanService.DeleteTaskAsync(
                    userId.Value,
                    historyId,
                    taskId
                );

                if (chatHistory == null)
                    return NotFound(new { message = "Chat history not found." });

                return Ok(new
                {
                    message = "Task deleted.",
                    chatHistory.Id,
                    progress = AiResponseMapper.CalculateProgress(chatHistory.Tasks)
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Task not found." });
            }
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