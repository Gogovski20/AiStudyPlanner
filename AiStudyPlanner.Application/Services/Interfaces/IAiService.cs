using AiStudyPlanner.Domain.Models;

namespace AiStudyPlanner.Application.Services.Interfaces
{
    public interface IAiService
    {
        Task<AiResponse?> GetStudyPlanAsync(string userInput);
    }
}
