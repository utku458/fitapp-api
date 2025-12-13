using System;

namespace FitApp.DTOs
{
    public class ArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public int EstimatedReadMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

