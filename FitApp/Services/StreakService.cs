using Microsoft.EntityFrameworkCore;
using FitApp.Data;
using FitApp.Models;

namespace FitApp.Services
{
    public class StreakService : IStreakService
    {
        private readonly ApplicationDbContext _context;

        public StreakService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CalculateStreakAsync(int todoId, DateTime date)
        {
            var todo = await _context.Todos.FindAsync(todoId);
            if (todo == null) return 0;

            var completions = await _context.TaskCompletions
                .Where(tc => tc.TodoId == todoId)
                .OrderByDescending(tc => tc.CompletionDate)
                .ToListAsync();

            if (!completions.Any()) return 0;

            int streak = 0;
            DateTime currentDate = date.Date;

            foreach (var completion in completions)
            {
                var completionDate = completion.CompletionDate.Date;
                
                if (IsConsecutiveDate(todo.TaskType, completionDate, currentDate))
                {
                    streak++;
                    currentDate = GetPreviousPeriodDate(todo.TaskType, completionDate);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        public async Task<bool> IsTaskActiveForDateAsync(Todo todo, DateTime date)
        {
            // Arşivlenmiş görevler aktif değil
            if (todo.IsArchived) return false;

            // Başlangıç tarihi kontrolü
            if (todo.StartDate.HasValue && todo.StartDate > date) return false;

            // Bitiş tarihi kontrolü
            if (todo.EndDate.HasValue && todo.EndDate < date) return false;

            return todo.TaskType switch
            {
                TaskType.Daily => true, // Günlük görevler her gün aktif
                
                TaskType.Weekly => IsWeeklyTaskActive(todo, date),
                
                TaskType.Monthly => IsMonthlyTaskActive(todo, date),
                
                TaskType.Goal => todo.TargetDate?.Date >= date.Date, // Hedef tarihine kadar aktif
                
                _ => false
            };
        }

        public async Task<bool> CompleteTaskAsync(int todoId, DateTime date)
        {
            var todo = await _context.Todos.FindAsync(todoId);
            if (todo == null) return false;

            // Görev o tarihte aktif mi kontrol et
            if (!await IsTaskActiveForDateAsync(todo, date)) return false;

            // Zaten tamamlanmış mı kontrol et
            if (await IsTaskCompletedForDateAsync(todoId, date)) return false;

            // Yeni tamamlanma kaydı oluştur
            var completion = new TaskCompletion
            {
                TodoId = todoId,
                CompletionDate = date.Date,
                CompletedAt = DateTime.UtcNow,
                TaskType = todo.TaskType,
                StreakAtCompletion = await CalculateStreakAsync(todoId, date) + 1
            };

            _context.TaskCompletions.Add(completion);

            // Streak'i güncelle
            todo.Streak = completion.StreakAtCompletion;
            todo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UncompleteTaskAsync(int todoId, DateTime date)
        {
            var completion = await _context.TaskCompletions
                .FirstOrDefaultAsync(tc => tc.TodoId == todoId && tc.CompletionDate.Date == date.Date);

            if (completion == null) return false;

            _context.TaskCompletions.Remove(completion);

            // Streak'i yeniden hesapla
            var todo = await _context.Todos.FindAsync(todoId);
            if (todo != null)
            {
                todo.Streak = await CalculateStreakAsync(todoId, date);
                todo.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsTaskCompletedForDateAsync(int todoId, DateTime date)
        {
            return await _context.TaskCompletions
                .AnyAsync(tc => tc.TodoId == todoId && tc.CompletionDate.Date == date.Date);
        }

        public async Task<List<DateTime>> GetCompletedDatesAsync(int todoId, DateTime startDate, DateTime endDate)
        {
            return await _context.TaskCompletions
                .Where(tc => tc.TodoId == todoId && 
                           tc.CompletionDate.Date >= startDate.Date && 
                           tc.CompletionDate.Date <= endDate.Date)
                .Select(tc => tc.CompletionDate.Date)
                .OrderBy(d => d)
                .ToListAsync();
        }

        #region Private Helper Methods

        private bool IsWeeklyTaskActive(Todo todo, DateTime date)
        {
            if (todo.RepeatDays == 0) return false;

            var dayOfWeek = GetDayOfWeekBit(date.DayOfWeek);
            return (todo.RepeatDays & dayOfWeek) != 0;
        }

        private bool IsMonthlyTaskActive(Todo todo, DateTime date)
        {
            if (!todo.StartDate.HasValue) return false;

            var daysSinceStart = (date.Date - todo.StartDate.Value.Date).Days;
            return daysSinceStart >= 0 && daysSinceStart % todo.IntervalDays == 0;
        }

        private bool IsConsecutiveDate(TaskType taskType, DateTime completionDate, DateTime currentDate)
        {
            return taskType switch
            {
                TaskType.Daily => (currentDate - completionDate).Days == 1,
                TaskType.Weekly => GetWeekDifference(completionDate, currentDate) == 1,
                TaskType.Monthly => GetMonthDifference(completionDate, currentDate) == 1,
                TaskType.Goal => true, // Hedef görevler için streak mantığı farklı
                _ => false
            };
        }

        private DateTime GetPreviousPeriodDate(TaskType taskType, DateTime date)
        {
            return taskType switch
            {
                TaskType.Daily => date.AddDays(-1),
                TaskType.Weekly => date.AddDays(-7),
                TaskType.Monthly => date.AddMonths(-1),
                TaskType.Goal => date, // Hedef görevler için
                _ => date
            };
        }

        private int GetDayOfWeekBit(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => 1,    // Pzt
                DayOfWeek.Tuesday => 2,   // Sal
                DayOfWeek.Wednesday => 4, // Çar
                DayOfWeek.Thursday => 8,  // Per
                DayOfWeek.Friday => 16,   // Cum
                DayOfWeek.Saturday => 32, // Cmt
                DayOfWeek.Sunday => 64,   // Paz
                _ => 0
            };
        }

        private int GetWeekDifference(DateTime date1, DateTime date2)
        {
            var startOfWeek1 = date1.AddDays(-(int)date1.DayOfWeek);
            var startOfWeek2 = date2.AddDays(-(int)date2.DayOfWeek);
            return (int)((startOfWeek2 - startOfWeek1).TotalDays / 7);
        }

        private int GetMonthDifference(DateTime date1, DateTime date2)
        {
            return (date2.Year - date1.Year) * 12 + (date2.Month - date1.Month);
        }

        #endregion
    }
}



