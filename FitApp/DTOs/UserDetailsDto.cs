using System.ComponentModel.DataAnnotations;

namespace FitApp.DTOs
{
    public class UserDetailsDto
   {
        public string? FitnessGoal { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public bool? HasTrainingExperience { get; set; }
        public int? FitnessLevel { get; set; }
        public int? DailyCalorieGoal { get; set; }
        public int? DailyWaterGoal { get; set; }
        public int? DailyStepGoal { get; set; }
        public int? WeeklyCommitment { get; set; }
    }
} 