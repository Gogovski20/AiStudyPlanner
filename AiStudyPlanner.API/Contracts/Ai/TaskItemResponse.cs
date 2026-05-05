namespace AiStudyPlanner.API.Contracts.Ai
{
    public class TaskItemResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}
