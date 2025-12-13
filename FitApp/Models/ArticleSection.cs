using System.ComponentModel.DataAnnotations;

namespace FitApp.Models
{
    public class ArticleSection
    {
        public int Id { get; set; }

        [Required]
        public int ArticleId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public int SectionOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Article Article { get; set; }
        public ICollection<ArticleImage> Images { get; set; } = new List<ArticleImage>();
        public ICollection<ArticleTip> Tips { get; set; } = new List<ArticleTip>();
        public ICollection<ArticleWarning> Warnings { get; set; } = new List<ArticleWarning>();
    }
}
