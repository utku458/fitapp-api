using FitApp.Models;

namespace FitApp.Services
{
    public interface IStreakService
    {
        Task<int> CalculateStreakAsync(int todoId, DateTime date);
        Task<bool> IsTaskActiveForDateAsync(Todo todo, DateTime date);
        Task<bool> CompleteTaskAsync(int todoId, DateTime date);
        Task<bool> UncompleteTaskAsync(int todoId, DateTime date);
        Task<bool> IsTaskCompletedForDateAsync(int todoId, DateTime date);
        Task<List<DateTime>> GetCompletedDatesAsync(int todoId, DateTime startDate, DateTime endDate);
    }
}



