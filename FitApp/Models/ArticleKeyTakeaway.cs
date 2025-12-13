using System.ComponentModel.DataAnnotations;

namespace FitApp.Models
{
    public class ArticleKeyTakeaway
    {
        public int Id { get; set; }

        [Required]
        public int ArticleId { get; set; }

        [Required]
        public string TakeawayText { get; set; }

        public int TakeawayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Article Article { get; set; }
    }
}
