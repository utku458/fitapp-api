using System.ComponentModel.DataAnnotations;

namespace FitApp.Models
{
    public class ArticleTip
    {
        public int Id { get; set; }

        [Required]
        public int ArticleId { get; set; }

        public int? SectionId { get; set; }

        [Required]
        public string TipText { get; set; }

        public int TipOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Article Article { get; set; }
        public ArticleSection Section { get; set; }
    }
}
