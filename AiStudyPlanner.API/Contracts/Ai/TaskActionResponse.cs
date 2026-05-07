namespace AiStudyPlanner.API.Contracts.Ai
{
    public class TaskActionResponse
    {
        public string Message { get; set; } = string.Empty;
        public int ChatHistoryId { get; set; }
        public TaskItemResponse? Task { get; set; }
        public ProgressResponse Progress { get; set; } = new();
    }
}
