using System;

namespace FitApp.DTOs
{
    public class DailyStatsDto
    {
        public int? Id { get; set; } // Update için opsiyonel
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public int WaterIntake { get; set; }
        public int StepCount { get; set; }
        public bool DidWorkout { get; set; }
        public int Calories { get; set; }
    }
} 