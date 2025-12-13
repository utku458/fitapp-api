using System.ComponentModel.DataAnnotations;

namespace FitApp.Models
{
    public class ArticleReference
    {
        public int Id { get; set; }

        [Required]
        public int ArticleId { get; set; }

        [Required]
        public string ReferenceText { get; set; }

        public string ReferenceUrl { get; set; }

        public int ReferenceOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Article Article { get; set; }
    }
}
