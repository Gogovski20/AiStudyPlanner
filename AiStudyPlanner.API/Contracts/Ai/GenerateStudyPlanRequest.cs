using System.ComponentModel.DataAnnotations;

namespace AiStudyPlanner.API.Contracts.Ai
{
    public class GenerateStudyPlanRequest
    {
        [Required]
        [MinLength(5)]
        [MaxLength(1000)]
        public string Input { get; set; } = string.Empty;
    }
}
