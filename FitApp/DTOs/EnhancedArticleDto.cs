using System.Text.Json;

namespace FitApp.DTOs
{
    public class EnhancedArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public int EstimatedReadMinutes { get; set; }
        public string CreatedAt { get; set; }
        
        // Enhanced fields
        public string Content { get; set; }
        public string DifficultyLevel { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<int> RelatedArticles { get; set; } = new List<int>();
        public int ViewCount { get; set; }
        public decimal Rating { get; set; }
        public bool IsFeatured { get; set; }
        
        // Related data
        public List<ArticleSectionDto> Sections { get; set; } = new List<ArticleSectionDto>();
        public List<string> KeyTakeaways { get; set; } = new List<string>();
        public List<ArticleReferenceDto> References { get; set; } = new List<ArticleReferenceDto>();
    }

    public class ArticleSectionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int SectionOrder { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public List<string> Tips { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class ArticleReferenceDto
    {
        public int Id { get; set; }
        public string ReferenceText { get; set; }
        public string ReferenceUrl { get; set; }
        public int ReferenceOrder { get; set; }
    }

    public class RatingDto
    {
        public decimal Rating { get; set; }
    }
}
