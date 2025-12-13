using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitApp.Models
{
    public class TodoCompletion
    {
        public int Id { get; set; }
        
        [Required]
        public int TodoId { get; set; }
        
        [Required]
        public DateTime CompletionDate { get; set; }
        
        [Required]
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Todo Todo { get; set; } = null!;
    }
}
