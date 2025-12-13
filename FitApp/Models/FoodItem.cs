using System.ComponentModel.DataAnnotations;

namespace FitApp.Models
{
    public class FoodItem
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int CaloriesPer100g { get; set; }

        [Required]
        public int ProteinPer100g { get; set; }

        [Required]
        public int CarbsPer100g { get; set; }

        [Required]
        public int FatPer100g { get; set; }

        public string? Locale { get; set; } = "tr-TR";

        public string? Brand { get; set; }

        public string? Category { get; set; }

        public string? PortionName { get; set; }

        public int? PortionGrams { get; set; }
    }
}


