using System;
using System.Text.Json;

namespace FitApp.Models
{
    public class WorkoutProgram
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Duration { get; set; }
        public string Difficulty { get; set; }
        public string FocusArea { get; set; }
        public string ProgramType { get; set; }
        public int WeeklyFrequency { get; set; }
        public string EstimatedDuration { get; set; }
        public string Equipment { get; set; } // JSON string
        public string TargetMuscles { get; set; } // JSON string
        public string ProgramOverview { get; set; }
        public string Benefits { get; set; } // JSON string
        public string Tips { get; set; } // JSON string
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<WorkoutProgramExercise> WorkoutProgramExercises { get; set; } = new List<WorkoutProgramExercise>();
    }
}


