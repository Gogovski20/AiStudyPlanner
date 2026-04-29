namespace AiStudyPlanner.API.Models
{
    public class AiResponse
    {
        public List<string> Tasks { get; set; } = new List<string>();
        public string EstimatedTime { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }
}
