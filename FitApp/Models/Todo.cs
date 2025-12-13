using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitApp.Models
{
    public enum TodoCategory
    {
        General = 0,
        Work = 1,
        Personal = 2,
        Health = 3,
        Education = 4
    }

    public enum TaskType
    {
        Daily = 0,    // Her gün otomatik aktif
        Weekly = 1,   // Haftanın seçilen günlerinde tekrarlayan
        Monthly = 2,  // Belirli aralıklarla (her 30 günde bir) tekrar eden
        Goal = 3      // Uzun vadeli hedefler
    }

    public class Todo
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(120, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public TodoCategory Category { get; set; }
        
        [Required]
        public TaskType TaskType { get; set; }
        
        /// <summary>
        /// Haftalık görevler için hangi günler aktif (Pzt=1, Sal=2, Çar=4, Per=8, Cum=16, Cmt=32, Paz=64)
        /// </summary>
        public int RepeatDays { get; set; } = 0;
        
        /// <summary>
        /// Aylık görevler için tekrar aralığı (gün cinsinden, örn: 30)
        /// </summary>
        public int IntervalDays { get; set; } = 30;
        
        /// <summary>
        /// Tamamlanan ardışık gün/hafta/ay sayısı
        /// </summary>
        public int Streak { get; set; } = 0;
        
        /// <summary>
        /// Görevin tamamlanma durumu
        /// </summary>
        public bool IsCompleted { get; set; } = false;
        
        /// <summary>
        /// Tek seferlik todo'lar için due date, haftalık için başlangıç tarihi
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// Haftalık todo'lar için bitiş tarihi (opsiyonel)
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Hedef tabanlı todo'lar için hedef tarihi
        /// </summary>
        public DateTime? TargetDate { get; set; }
        
        /// <summary>
        /// Hedef tabanlı todo'lar için mevcut ilerleme (0-100)
        /// </summary>
        public int ProgressPercentage { get; set; } = 0;
        
        /// <summary>
        /// Hedef tabanlı todo'lar için başlangıç değeri
        /// </summary>
        public string? StartingValue { get; set; }
        
        /// <summary>
        /// Hedef tabanlı todo'lar için hedef değeri
        /// </summary>
        public string? TargetValue { get; set; }
        
        /// <summary>
        /// Soft delete için - true ise arşivlenmiş
        /// </summary>
        public bool IsArchived { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public User User { get; set; } = null!;
    }

}
