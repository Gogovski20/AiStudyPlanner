namespace AiStudyPlanner.API.Contracts.Ai
{
    public class ChatHistoryResponse
    {
        public int Id { get; set; }
        public string UserInput { get; set; } = string.Empty;
        public List<TaskItemResponse> Tasks { get; set; } = new();
        public string EstimatedTime { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ProgressResponse Progress { get; set; } = new();
    }
}
