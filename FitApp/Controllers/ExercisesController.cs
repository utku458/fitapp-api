using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitApp.Data;
using FitApp.DTOs;
using FitApp.Models;
using System.Text.Json;

namespace FitApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExercisesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExercisesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/exercises?query=&muscleGroup=&equipment=&page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? query,
            [FromQuery] string? muscleGroup,
            [FromQuery] string? equipment,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var q = _context.Exercises.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = query.Trim();
                q = q.Where(e => e.Name.Contains(like) || e.Description.Contains(like));
            }
            if (!string.IsNullOrWhiteSpace(muscleGroup))
            {
                q = q.Where(e => e.MuscleGroup == muscleGroup);
            }
            if (!string.IsNullOrWhiteSpace(equipment))
            {
                q = q.Where(e => e.Equipment == equipment);
            }

            var total = await q.CountAsync();
            var items = await q
                .OrderBy(e => e.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = items.Select(e => new ExerciseDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                MuscleGroup = e.MuscleGroup,
                Equipment = e.Equipment,
                Sets = e.Sets,
                Reps = e.Reps,
                RestTime = e.RestTime,
                ImageUrl = e.ImageUrl,
                VideoUrl = e.VideoUrl,
                Instructions = !string.IsNullOrEmpty(e.Instructions) ? JsonSerializer.Deserialize<List<string>>(e.Instructions) : new List<string>(),
                Tips = !string.IsNullOrEmpty(e.Tips) ? JsonSerializer.Deserialize<List<string>>(e.Tips) : new List<string>(),
                OrderIndex = 0
            }).ToList();

            return Ok(new { exercises = result, totalCount = total, page, pageSize, totalPages = (int)Math.Ceiling((double)total / pageSize) });
        }

        // GET: api/exercises/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var e = await _context.Exercises.FindAsync(id);
            if (e == null) return NotFound();

            var result = new ExerciseDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                MuscleGroup = e.MuscleGroup,
                Equipment = e.Equipment,
                Sets = e.Sets,
                Reps = e.Reps,
                RestTime = e.RestTime,
                ImageUrl = e.ImageUrl,
                VideoUrl = e.VideoUrl,
                Instructions = !string.IsNullOrEmpty(e.Instructions) ? JsonSerializer.Deserialize<List<string>>(e.Instructions) : new List<string>(),
                Tips = !string.IsNullOrEmpty(e.Tips) ? JsonSerializer.Deserialize<List<string>>(e.Tips) : new List<string>(),
                OrderIndex = 0
            };

            return Ok(result);
        }

        // POST: api/exercises
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ExerciseDto dto)
        {
            var e = new Exercise
            {
                Name = dto.Name,
                Description = dto.Description,
                MuscleGroup = dto.MuscleGroup,
                Equipment = dto.Equipment,
                Sets = dto.Sets,
                Reps = dto.Reps,
                RestTime = dto.RestTime,
                ImageUrl = dto.ImageUrl,
                VideoUrl = dto.VideoUrl,
                Instructions = JsonSerializer.Serialize(dto.Instructions),
                Tips = JsonSerializer.Serialize(dto.Tips)
            };

            _context.Exercises.Add(e);
            await _context.SaveChangesAsync();

            dto.Id = e.Id;
            return CreatedAtAction(nameof(GetById), new { id = e.Id }, dto);
        }

        // PUT: api/exercises/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ExerciseDto dto)
        {
            var e = await _context.Exercises.FindAsync(id);
            if (e == null) return NotFound();

            e.Name = dto.Name;
            e.Description = dto.Description;
            e.MuscleGroup = dto.MuscleGroup;
            e.Equipment = dto.Equipment;
            e.Sets = dto.Sets;
            e.Reps = dto.Reps;
            e.RestTime = dto.RestTime;
            e.ImageUrl = dto.ImageUrl;
            e.VideoUrl = dto.VideoUrl;
            e.Instructions = JsonSerializer.Serialize(dto.Instructions);
            e.Tips = JsonSerializer.Serialize(dto.Tips);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/exercises/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var e = await _context.Exercises.FindAsync(id);
            if (e == null) return NotFound();

            _context.Exercises.Remove(e);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
