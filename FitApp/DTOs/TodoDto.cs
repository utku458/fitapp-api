using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.Json;
using FitApp.Models;

namespace FitApp.DTOs
{
    public class TodoDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TodoCategory Category { get; set; }
        public TaskType TaskType { get; set; }
        public int RepeatDays { get; set; }
        public int IntervalDays { get; set; }
        public int Streak { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? TargetDate { get; set; }
        public int ProgressPercentage { get; set; }
        public string? StartingValue { get; set; }
        public string? TargetValue { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Haftalık todo'lar için günlük tamamlanma durumu
        public bool IsCompletedForDate { get; set; }
        public DateTime? CompletedAtForDate { get; set; }
    }

    public class CreateTodoDto
    {
        [Required]
        [StringLength(120, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("category")]
        public TodoCategory Category { get; set; }
        
        /// <summary>
        /// Client'dan gelen category (Category ile aynı)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("categoryType")]
        public TodoCategory? CategoryType { get; set; }
        
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("taskType")]
        public TaskType TaskType { get; set; }
        
        /// <summary>
        /// Haftalık görevler için hangi günler aktif (Pzt=1, Sal=2, Çar=4, Per=8, Cum=16, Cmt=32, Paz=64)
        /// </summary>
        [JsonPropertyName("repeatDays")]
        public int RepeatDays { get; set; } = 0;
        
        /// <summary>
        /// Aylık görevler için tekrar aralığı (gün cinsinden)
        /// </summary>
        [JsonPropertyName("intervalDays")]
        public int IntervalDays { get; set; } = 30;
        
        /// <summary>
        /// Tek seferlik todo'lar için due date, haftalık için başlangıç tarihi
        /// </summary>
        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// Haftalık todo'lar için bitiş tarihi (opsiyonel)
        /// </summary>
        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Hedef tabanlı todo'lar için hedef tarihi
        /// </summary>
        [JsonPropertyName("targetDate")]
        public DateTime? TargetDate { get; set; }
        
        /// <summary>
        /// Hedef tabanlı todo'lar için başlangıç değeri
        /// </summary>
        [JsonPropertyName("startingValue")]
        public string? StartingValue { get; set; }
        
        /// <summary>
        /// Hedef tabanlı todo'lar için hedef değeri
        /// </summary>
        [JsonPropertyName("targetValue")]
        public string? TargetValue { get; set; }
    }

    public class UpdateTodoDto
    {
        [StringLength(120, MinimumLength = 1)]
        public string? Title { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("category")]
        public TodoCategory? Category { get; set; }
        
        /// <summary>
        /// Client'dan gelen category (Category ile aynı)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("categoryType")]
        public TodoCategory? CategoryType { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("taskType")]
        public TaskType? TaskType { get; set; }
        
        [JsonPropertyName("repeatDays")]
        public int? RepeatDays { get; set; }
        
        [JsonPropertyName("intervalDays")]
        public int? IntervalDays { get; set; }
        
        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }
        
        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }
        
        [JsonPropertyName("targetDate")]
        public DateTime? TargetDate { get; set; }
        
        [JsonPropertyName("startingValue")]
        public string? StartingValue { get; set; }
        
        [JsonPropertyName("targetValue")]
        public string? TargetValue { get; set; }
    }

    public class TodoCompletionDto
    {
        public int Id { get; set; }
        public int TodoId { get; set; }
        public DateTime Date { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    public class CompleteTodoDto
    {
        [Required]
        public DateTime Date { get; set; }
    }

    public class UpdateProgressDto
    {
        [Required]
        [Range(0, 100)]
        public int ProgressPercentage { get; set; }
        
        public string? CurrentValue { get; set; }
    }

    public class TodoListResponse
    {
        public List<TodoDto> Todos { get; set; } = new List<TodoDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ActiveTodosResponse
    {
        public List<TodoDto> ActiveTodos { get; set; } = new List<TodoDto>();
        public DateTime Date { get; set; }
        public int TotalCount { get; set; }
    }
}
