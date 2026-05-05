namespace AiStudyPlanner.API.Contracts.Ai
{
    public class GenerateStudyPlanResponse
    {
        public int Id { get; set; }
        public List<TaskItemResponse> Tasks { get; set; } = new();
        public string EstimatedTime { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public ProgressResponse Progress { get; set; } = new();
    }
}
