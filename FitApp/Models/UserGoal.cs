using System.ComponentModel.DataAnnotations.Schema;

namespace FitApp.Models
{
    public class UserGoal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DailyStepGoal { get; set; }
        public int DailyWaterGoal { get; set; }
        public int DailyCalorieGoal { get; set; }
        public int WeeklyWorkoutDays { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
