namespace AiStudyPlanner.Domain.Models
{
    public class AiResponse
    {
        public List<TaskItem> Tasks { get; set; } = new();
        public string EstimatedTime { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }
}
