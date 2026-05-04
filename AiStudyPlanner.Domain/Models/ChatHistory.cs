namespace AiStudyPlanner.Domain.Models
{
    public class ChatHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserInput { get; set; } = string.Empty;

        public List<TaskItem> Tasks { get; set; } = new(); 
        public string EstimatedTime { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User User { get; set; } = null!;
    }
}
