namespace FitApp.DTOs
{
    public class UserDataDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // Hashlenmiş hali, güvenlik için normalde dönülmez ama modelde var
        public string? FitnessGoal { get; set; }
        public string? Gender { get; set; }
        public int? Weight { get; set; }
        public int? Age { get; set; }
        public int? Height { get; set; }
        public bool? HasTrainingExperience { get; set; }
        public int? FitnessLevel { get; set; }
        public int? DailyCalorieGoal { get; set; }
        public int? DailyWaterGoal { get; set; }
        public int? DailyStepGoal { get; set; }
        public int? WeeklyCommitment { get; set; }
    }
} 