using System.ComponentModel.DataAnnotations;

namespace FitApp.Models
{
    public class ArticleImage
    {
        public int Id { get; set; }

        [Required]
        public int ArticleId { get; set; }

        public int? SectionId { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public string AltText { get; set; }

        public int ImageOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Article Article { get; set; }
        public ArticleSection Section { get; set; }
    }
}
