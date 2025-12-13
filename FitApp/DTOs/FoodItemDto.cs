namespace FitApp.DTOs
{
    public class FoodItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CaloriesPer100g { get; set; }
        public int ProteinPer100g { get; set; }
        public int CarbsPer100g { get; set; }
        public int FatPer100g { get; set; }
        public string? Locale { get; set; }
        public string? Brand { get; set; }
        public string? Category { get; set; }
        public string? PortionName { get; set; }
        public int? PortionGrams { get; set; }
    }
}


