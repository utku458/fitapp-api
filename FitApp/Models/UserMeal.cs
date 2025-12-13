using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitApp.Models
{
    public class UserMeal
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public int Calories { get; set; }
        [Required]
        public int Protein { get; set; }
        [Required]
        public int Carbs { get; set; }
        [Required]
        public int Fat { get; set; }
        [Required]
        public string MealTime { get; set; } // Breakfast, Lunch, Dinner

        [Column(TypeName = "date")]
        public DateTime Date { get; set; } // Sadece gün, saat olmadan
    }
} 