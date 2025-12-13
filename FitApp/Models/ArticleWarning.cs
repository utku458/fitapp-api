using System.ComponentModel.DataAnnotations;

namespace FitApp.Models
{
    public class ArticleWarning
    {
        public int Id { get; set; }

        [Required]
        public int ArticleId { get; set; }

        public int? SectionId { get; set; }

        [Required]
        public string WarningText { get; set; }

        public int WarningOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Article Article { get; set; }
        public ArticleSection Section { get; set; }
    }
}
