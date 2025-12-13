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
    public class UserDailyStatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserDailyStatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/userdailystats/{userId}/today
        [HttpGet("{userId}/today")]
        public async Task<ActionResult<DailyStatsDto>> GetToday(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var stats = await _context.UserDailyStats
                .Where(x => x.UserId == userId && x.Date == today)
                .FirstOrDefaultAsync();
            if (stats == null) return NotFound();
            return Ok(new DailyStatsDto
            {
                Id = stats.Id,
                UserId = stats.UserId,
                Date = stats.Date,
                WaterIntake = stats.WaterIntake,
                StepCount = stats.StepCount,
                DidWorkout = stats.DidWorkout,
                Calories = stats.Calories
            });
        }

        // GET: api/userdailystats/{userId}?start=...&end=...
        [HttpGet("{userId}")]
        public async Task<ActionResult> GetRange(int userId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var stats = await _context.UserDailyStats
                .Where(x => x.UserId == userId && x.Date >= start.Date && x.Date <= end.Date)
                .OrderBy(x => x.Date)
                .Select(stats => new DailyStatsDto
                {
                    Id = stats.Id,
                    UserId = stats.UserId,
                    Date = stats.Date,
                    WaterIntake = stats.WaterIntake,
                    StepCount = stats.StepCount,
                    DidWorkout = stats.DidWorkout,
                    Calories = stats.Calories
                })
                .ToListAsync();
            return Ok(stats);
        }

        // POST: api/userdailystats
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] DailyStatsDto dto)
        {
            var date = dto.Date.Date;
            var exists = await _context.UserDailyStats.AnyAsync(x => x.UserId == dto.UserId && x.Date == date);
            if (exists)
                return Conflict("Bu kullanıcı için bu tarihte veri zaten mevcut.");

            var stats = new UserDailyStats
            {
                UserId = dto.UserId,
                Date = date,
                WaterIntake = dto.WaterIntake,
                StepCount = dto.StepCount,
                DidWorkout = dto.DidWorkout,
                Calories = dto.Calories
            };
            _context.UserDailyStats.Add(stats);
            await _context.SaveChangesAsync();
            dto.Id = stats.Id;
            return CreatedAtAction(nameof(GetToday), new { userId = dto.UserId }, dto);
        }

        // PUT: api/userdailystats/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] DailyStatsDto dto)
        {
            var stats = await _context.UserDailyStats.FindAsync(id);
            if (stats == null) return NotFound();
            if (stats.UserId != dto.UserId)
                return BadRequest("UserId değiştirilemez.");
            // Tarih değişirse unique constraint çakışmasın diye kontrol
            if (stats.Date.Date != dto.Date.Date)
            {
                var exists = await _context.UserDailyStats.AnyAsync(x => x.UserId == dto.UserId && x.Date == dto.Date.Date && x.Id != id);
                if (exists)
                    return Conflict("Bu kullanıcı için bu tarihte veri zaten mevcut.");
            }
            stats.Date = dto.Date.Date;
            stats.WaterIntake = dto.WaterIntake;
            stats.StepCount = dto.StepCount;
            stats.DidWorkout = dto.DidWorkout;
            stats.Calories = dto.Calories;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 