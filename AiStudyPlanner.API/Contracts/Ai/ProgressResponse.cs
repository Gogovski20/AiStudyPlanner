namespace AiStudyPlanner.API.Contracts.Ai
{
    public class ProgressResponse
    {
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public double Percentage { get; set; }
    }
}
