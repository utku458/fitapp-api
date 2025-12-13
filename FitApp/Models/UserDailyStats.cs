using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitApp.Models
{
    public class UserDailyStats
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public DateTime Date { get; set; } // Sadece gün, saat olmadan

        [Required]
        public int WaterIntake { get; set; } // mL

        [Required]
        public int StepCount { get; set; }

        [Required]
        public bool DidWorkout { get; set; }

        [Required]
        public int Calories { get; set; }
    }
} 