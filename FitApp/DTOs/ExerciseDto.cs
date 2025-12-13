using System.Text.Json;

namespace FitApp.DTOs
{
    public class ExerciseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MuscleGroup { get; set; } = string.Empty;
        public string Equipment { get; set; } = string.Empty;
        public int Sets { get; set; }
        public string Reps { get; set; } = string.Empty;
        public string RestTime { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public List<string> Instructions { get; set; } = new List<string>();
        public List<string> Tips { get; set; } = new List<string>();
        public int OrderIndex { get; set; }
    }
}
