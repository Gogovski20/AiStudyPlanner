using System.ComponentModel.DataAnnotations;

namespace AiStudyPlanner.API.Contracts.Ai
{
    public class AddTaskRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;
    }
}
