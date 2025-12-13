using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitApp.Models
{
    public class TaskCompletion
    {
        public int Id { get; set; }
        
        [Required]
        public int TodoId { get; set; }
        
        [Required]
        public DateTime CompletionDate { get; set; }
        
        [Required]
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Bu tamamlanma sırasındaki streak değeri
        /// </summary>
        public int StreakAtCompletion { get; set; }
        
        /// <summary>
        /// Tamamlanma türü (günlük, haftalık, aylık)
        /// </summary>
        public TaskType TaskType { get; set; }
        
        // Navigation properties
        public Todo Todo { get; set; } = null!;
    }
}



