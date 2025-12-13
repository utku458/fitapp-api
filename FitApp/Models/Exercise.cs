using System.Text.Json;

namespace FitApp.Models
{
    public class Exercise
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
        public string Instructions { get; set; } = string.Empty; // JSON string
        public string Tips { get; set; } = string.Empty; // JSON string
        
        // Navigation properties
        public ICollection<WorkoutProgramExercise> WorkoutProgramExercises { get; set; } = new List<WorkoutProgramExercise>();
    }
}
