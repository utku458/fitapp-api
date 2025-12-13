using System.Text.Json;

namespace FitApp.DTOs
{
    public class WorkoutProgramDto
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
        public List<string> Equipment { get; set; } = new List<string>();
        public List<string> TargetMuscles { get; set; } = new List<string>();
        public string ProgramOverview { get; set; }
        public List<string> Benefits { get; set; } = new List<string>();
        public List<string> Tips { get; set; } = new List<string>();
        public List<ExerciseDto> Exercises { get; set; } = new List<ExerciseDto>();
    }
}


