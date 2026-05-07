using AiStudyPlanner.API.Contracts.Ai;
using AiStudyPlanner.API.Contracts.Common;
using AiStudyPlanner.API.Mappers;
using AiStudyPlanner.Application.Services.Interfaces;
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
                return BadRequest(new ErrorResponse { Message = "Input cannot be empty." });

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
                    return StatusCode(503, new ErrorResponse { Message = ex.Message });

                return StatusCode(500, new ErrorResponse
                {
                    Message = "Something went wrong while generating the study plan."
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
                return NotFound(new ErrorResponse { Message = "Chat history not found." });

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
                    return NotFound(new ErrorResponse { Message = "Chat history not found." });

                var updatedTask = chatHistory.Tasks.First(t => t.Id == taskId);

                return Ok(new TaskActionResponse
                {
                    Message = "Task marked as completed.",
                    ChatHistoryId = chatHistory.Id,
                    Task = AiResponseMapper.ToTaskItemResponse(updatedTask),
                    Progress = AiResponseMapper.CalculateProgress(chatHistory.Tasks)
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponse { Message = "Task not found." });
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
                    return NotFound(new ErrorResponse { Message = "Chat history not found." });

                var updatedTask = chatHistory.Tasks.First(t => t.Id == taskId);

                return Ok(new TaskActionResponse
                {
                    Message = "Task marked as incomplete.",
                    ChatHistoryId = chatHistory.Id,
                    Task = AiResponseMapper.ToTaskItemResponse(updatedTask),
                    Progress = AiResponseMapper.CalculateProgress(chatHistory.Tasks)
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponse { Message = "Task not found." });
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
                    return NotFound(new ErrorResponse { Message = "Chat history not found." });

                var updatedTask = chatHistory.Tasks.First(t => t.Id == taskId);

                return Ok(new TaskActionResponse
                {
                    Message = "Task title updated.",
                    ChatHistoryId = chatHistory.Id,
                    Task = AiResponseMapper.ToTaskItemResponse(updatedTask),
                    Progress = AiResponseMapper.CalculateProgress(chatHistory.Tasks)
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponse { Message = "Task not found." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
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
                    return NotFound(new ErrorResponse { Message = "Chat history not found." });

                return Ok(new TaskActionResponse
                {
                    Message = "Task deleted.",
                    ChatHistoryId = chatHistory.Id,
                    Task = null,
                    Progress = AiResponseMapper.CalculateProgress(chatHistory.Tasks)
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponse { Message = "Task not found." });
            }
        }

        [HttpPost("history/{historyId:int}/tasks")]
        public async Task<IActionResult> AddTask(
            int historyId,
            [FromBody] AddTaskRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            try
            {
                var chatHistory = await _studyPlanService.AddTaskAsync(
                    userId.Value,
                    historyId,
                    request.Title
                );

                if (chatHistory == null)
                    return NotFound(new ErrorResponse { Message = "Chat history not found." });

                var addedTask = chatHistory.Tasks.Last();

                return Ok(new TaskActionResponse
                {
                    Message = "Task added.",
                    ChatHistoryId = chatHistory.Id,
                    Task = AiResponseMapper.ToTaskItemResponse(addedTask),
                    Progress = AiResponseMapper.CalculateProgress(chatHistory.Tasks)
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpDelete("history/{historyId:int}")]
        public async Task<IActionResult> DeleteHistory(int historyId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var deleted = await _studyPlanService.DeleteHistoryAsync(
                userId.Value,
                historyId
            );

            if (!deleted)
                return NotFound(new ErrorResponse { Message = "Chat history not found." });

            return Ok(new { message = "Study plan deleted." });
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