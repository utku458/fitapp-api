using System;
using System.ComponentModel.DataAnnotations;

namespace FitApp.Models
{
    public class Article
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Author { get; set; }

        // Nutrition, Cardio, Workout, Recovery, Sleep, Supplements
        [Required]
        public string Category { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public int EstimatedReadMinutes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Enhanced fields
        public string Content { get; set; }
        public string DifficultyLevel { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced
        public string Tags { get; set; } // JSON string for tags array
        public string RelatedArticles { get; set; } // JSON string for related article IDs
        public int ViewCount { get; set; } = 0;
        public decimal Rating { get; set; } = 0.0m;
        public bool IsFeatured { get; set; } = false;

        // Navigation properties
        public ICollection<ArticleSection> Sections { get; set; } = new List<ArticleSection>();
        public ICollection<ArticleImage> Images { get; set; } = new List<ArticleImage>();
        public ICollection<ArticleTip> Tips { get; set; } = new List<ArticleTip>();
        public ICollection<ArticleWarning> Warnings { get; set; } = new List<ArticleWarning>();
        public ICollection<ArticleKeyTakeaway> KeyTakeaways { get; set; } = new List<ArticleKeyTakeaway>();
        public ICollection<ArticleReference> References { get; set; } = new List<ArticleReference>();
    }
}

