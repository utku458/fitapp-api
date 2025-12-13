using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitApp.Data;
using FitApp.DTOs;
using FitApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FitApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserMealsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserMealsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/usermeals/{userId}/today
        [HttpGet("{userId}/today")]
        public async Task<ActionResult> GetToday(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var meals = await _context.UserMeals
                .Where(x => x.UserId == userId && x.Date == today)
                .OrderBy(x => x.MealTime)
                .Select(m => new MealDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    Name = m.Name,
                    Calories = m.Calories,
                    Protein = m.Protein,
                    Carbs = m.Carbs,
                    Fat = m.Fat,
                    MealTime = m.MealTime,
                    Date = m.Date
                })
                .ToListAsync();
            return Ok(meals);
        }

        // GET: api/usermeals/{userId}?start=...&end=...
        [HttpGet("{userId}")]
        public async Task<ActionResult> GetRange(int userId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var meals = await _context.UserMeals
                .Where(x => x.UserId == userId && x.Date >= start.Date && x.Date <= end.Date)
                .OrderBy(x => x.Date).ThenBy(x => x.MealTime)
                .Select(m => new MealDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    Name = m.Name,
                    Calories = m.Calories,
                    Protein = m.Protein,
                    Carbs = m.Carbs,
                    Fat = m.Fat,
                    MealTime = m.MealTime,
                    Date = m.Date
                })
                .ToListAsync();
            return Ok(meals);
        }

        // POST: api/usermeals
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] MealDto dto)
        {
            await EnsureDailyStatsExists(dto.UserId, dto.Date);
            var meal = new UserMeal
            {
                UserId = dto.UserId,
                Name = dto.Name,
                Calories = dto.Calories,
                Protein = dto.Protein,
                Carbs = dto.Carbs,
                Fat = dto.Fat,
                MealTime = dto.MealTime,
                Date = dto.Date.Date
            };
            _context.UserMeals.Add(meal);
            await _context.SaveChangesAsync();
            dto.Id = meal.Id;
            await RecalculateDailyStatsCalories(dto.UserId, dto.Date);
            return CreatedAtAction(nameof(GetToday), new { userId = dto.UserId }, dto);
        }

        // PUT: api/usermeals/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] MealDto dto)
        {
            var meal = await _context.UserMeals.FindAsync(id);
            if (meal == null) return NotFound();
            if (meal.UserId != dto.UserId)
                return BadRequest("UserId değiştirilemez.");
            meal.Name = dto.Name;
            meal.Calories = dto.Calories;
            meal.Protein = dto.Protein;
            meal.Carbs = dto.Carbs;
            meal.Fat = dto.Fat;
            meal.MealTime = dto.MealTime;
            meal.Date = dto.Date.Date;
            await _context.SaveChangesAsync();
            await RecalculateDailyStatsCalories(dto.UserId, dto.Date);
            return NoContent();
        }

        // DELETE: api/usermeals/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var meal = await _context.UserMeals.FindAsync(id);
            if (meal == null) return NotFound();
            _context.UserMeals.Remove(meal);
            await _context.SaveChangesAsync();
            await RecalculateDailyStatsCalories(meal.UserId, meal.Date);
            return NoContent();
        }

        private async Task RecalculateDailyStatsCalories(int userId, DateTime date)
        {
            var totalCalories = await _context.UserMeals
                .Where(m => m.UserId == userId && m.Date.Date == date.Date)
                .SumAsync(m => m.Calories);

            var stats = await _context.UserDailyStats
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Date.Date == date.Date);

            if (stats != null)
            {
                stats.Calories = totalCalories;
                await _context.SaveChangesAsync();
            }
        }

        private async Task EnsureDailyStatsExists(int userId, DateTime date)
        {
            var exists = await _context.UserDailyStats
                .AnyAsync(s => s.UserId == userId && s.Date.Date == date.Date);

            if (!exists)
            {
                var newStats = new UserDailyStats
                {
                    UserId = userId,
                    Date = date.Date
                };
                _context.UserDailyStats.Add(newStats);
                await _context.SaveChangesAsync();
            }
        }
    }
} 