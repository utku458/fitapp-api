using System;

namespace FitApp.DTOs
{
    public class MealDto
    {
        public int? Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }
        public string MealTime { get; set; }
        public DateTime Date { get; set; }
    }
} 