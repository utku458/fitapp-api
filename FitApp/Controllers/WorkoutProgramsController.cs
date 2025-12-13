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
    public class WorkoutProgramsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorkoutProgramsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/workoutprograms?query=&difficulty=&focusArea=&programType=&targetMuscle=&page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? query, 
            [FromQuery] string? difficulty, 
            [FromQuery] string? focusArea,
            [FromQuery] string? programType,
            [FromQuery] string? targetMuscle,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            var q = _context.WorkoutPrograms
                .Include(wp => wp.WorkoutProgramExercises)
                    .ThenInclude(wpe => wpe.Exercise)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = query.Trim();
                q = q.Where(w => w.Title.Contains(like) || w.ProgramOverview.Contains(like));
            }
            if (!string.IsNullOrWhiteSpace(difficulty))
            {
                q = q.Where(w => w.Difficulty == difficulty);
            }
            if (!string.IsNullOrWhiteSpace(focusArea))
            {
                q = q.Where(w => w.FocusArea == focusArea);
            }
            if (!string.IsNullOrWhiteSpace(programType))
            {
                q = q.Where(w => w.ProgramType == programType);
            }
            if (!string.IsNullOrWhiteSpace(targetMuscle))
            {
                q = q.Where(w => w.TargetMuscles.Contains(targetMuscle));
            }

            var total = await q.CountAsync();
            var items = await q
                .OrderByDescending(w => w.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = items.Select(w => new WorkoutProgramDto
            {
                Id = w.Id,
                Title = w.Title,
                ImageUrl = w.ImageUrl,
                Duration = w.Duration,
                Difficulty = w.Difficulty,
                FocusArea = w.FocusArea,
                ProgramType = w.ProgramType,
                WeeklyFrequency = w.WeeklyFrequency,
                EstimatedDuration = w.EstimatedDuration,
                Equipment = !string.IsNullOrEmpty(w.Equipment) ? JsonSerializer.Deserialize<List<string>>(w.Equipment) : new List<string>(),
                TargetMuscles = !string.IsNullOrEmpty(w.TargetMuscles) ? JsonSerializer.Deserialize<List<string>>(w.TargetMuscles) : new List<string>(),
                ProgramOverview = w.ProgramOverview,
                Benefits = !string.IsNullOrEmpty(w.Benefits) ? JsonSerializer.Deserialize<List<string>>(w.Benefits) : new List<string>(),
                Tips = !string.IsNullOrEmpty(w.Tips) ? JsonSerializer.Deserialize<List<string>>(w.Tips) : new List<string>(),
                Exercises = w.WorkoutProgramExercises
                    .OrderBy(wpe => wpe.OrderIndex)
                    .Select(wpe => new ExerciseDto
                    {
                        Id = wpe.Exercise.Id,
                        Name = wpe.Exercise.Name,
                        Description = wpe.Exercise.Description,
                        MuscleGroup = wpe.Exercise.MuscleGroup,
                        Equipment = wpe.Exercise.Equipment,
                        Sets = wpe.Exercise.Sets,
                        Reps = wpe.Exercise.Reps,
                        RestTime = wpe.Exercise.RestTime,
                        ImageUrl = wpe.Exercise.ImageUrl,
                        VideoUrl = wpe.Exercise.VideoUrl,
                        Instructions = !string.IsNullOrEmpty(wpe.Exercise.Instructions) ? JsonSerializer.Deserialize<List<string>>(wpe.Exercise.Instructions) : new List<string>(),
                        Tips = !string.IsNullOrEmpty(wpe.Exercise.Tips) ? JsonSerializer.Deserialize<List<string>>(wpe.Exercise.Tips) : new List<string>(),
                        OrderIndex = wpe.OrderIndex
                    }).ToList()
            }).ToList();

            return Ok(new { workoutPrograms = result, totalCount = total, page, pageSize, totalPages = (int)Math.Ceiling((double)total / pageSize) });
        }

        // GET: api/workoutprograms/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var w = await _context.WorkoutPrograms
                .Include(wp => wp.WorkoutProgramExercises)
                    .ThenInclude(wpe => wpe.Exercise)
                .FirstOrDefaultAsync(wp => wp.Id == id);

            if (w == null) return NotFound();

            var result = new WorkoutProgramDto
            {
                Id = w.Id,
                Title = w.Title,
                ImageUrl = w.ImageUrl,
                Duration = w.Duration,
                Difficulty = w.Difficulty,
                FocusArea = w.FocusArea,
                ProgramType = w.ProgramType,
                WeeklyFrequency = w.WeeklyFrequency,
                EstimatedDuration = w.EstimatedDuration,
                Equipment = !string.IsNullOrEmpty(w.Equipment) ? JsonSerializer.Deserialize<List<string>>(w.Equipment) : new List<string>(),
                TargetMuscles = !string.IsNullOrEmpty(w.TargetMuscles) ? JsonSerializer.Deserialize<List<string>>(w.TargetMuscles) : new List<string>(),
                ProgramOverview = w.ProgramOverview,
                Benefits = !string.IsNullOrEmpty(w.Benefits) ? JsonSerializer.Deserialize<List<string>>(w.Benefits) : new List<string>(),
                Tips = !string.IsNullOrEmpty(w.Tips) ? JsonSerializer.Deserialize<List<string>>(w.Tips) : new List<string>(),
                Exercises = w.WorkoutProgramExercises
                    .OrderBy(wpe => wpe.OrderIndex)
                    .Select(wpe => new ExerciseDto
                    {
                        Id = wpe.Exercise.Id,
                        Name = wpe.Exercise.Name,
                        Description = wpe.Exercise.Description,
                        MuscleGroup = wpe.Exercise.MuscleGroup,
                        Equipment = wpe.Exercise.Equipment,
                        Sets = wpe.Exercise.Sets,
                        Reps = wpe.Exercise.Reps,
                        RestTime = wpe.Exercise.RestTime,
                        ImageUrl = wpe.Exercise.ImageUrl,
                        VideoUrl = wpe.Exercise.VideoUrl,
                        Instructions = !string.IsNullOrEmpty(wpe.Exercise.Instructions) ? JsonSerializer.Deserialize<List<string>>(wpe.Exercise.Instructions) : new List<string>(),
                        Tips = !string.IsNullOrEmpty(wpe.Exercise.Tips) ? JsonSerializer.Deserialize<List<string>>(wpe.Exercise.Tips) : new List<string>(),
                        OrderIndex = wpe.OrderIndex
                    }).ToList()
            };

            return Ok(result);
        }

        // GET: api/workoutprograms/{id}/exercises
        [HttpGet("{id}/exercises")]
        public async Task<IActionResult> GetExercises(int id)
        {
            var exercises = await _context.WorkoutProgramExercises
                .Where(wpe => wpe.WorkoutProgramId == id)
                .Include(wpe => wpe.Exercise)
                .OrderBy(wpe => wpe.OrderIndex)
                .ToListAsync();

            var result = exercises.Select(wpe => new ExerciseDto
            {
                Id = wpe.Exercise.Id,
                Name = wpe.Exercise.Name,
                Description = wpe.Exercise.Description,
                MuscleGroup = wpe.Exercise.MuscleGroup,
                Equipment = wpe.Exercise.Equipment,
                Sets = wpe.Exercise.Sets,
                Reps = wpe.Exercise.Reps,
                RestTime = wpe.Exercise.RestTime,
                ImageUrl = wpe.Exercise.ImageUrl,
                VideoUrl = wpe.Exercise.VideoUrl,
                Instructions = !string.IsNullOrEmpty(wpe.Exercise.Instructions) ? JsonSerializer.Deserialize<List<string>>(wpe.Exercise.Instructions) : new List<string>(),
                Tips = !string.IsNullOrEmpty(wpe.Exercise.Tips) ? JsonSerializer.Deserialize<List<string>>(wpe.Exercise.Tips) : new List<string>(),
                OrderIndex = wpe.OrderIndex
            }).ToList();

            return Ok(result);
        }

        // POST: api/workoutprograms
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] WorkoutProgramDto dto)
        {
            var w = new WorkoutProgram
            {
                Title = dto.Title,
                ImageUrl = dto.ImageUrl,
                Duration = dto.Duration,
                Difficulty = dto.Difficulty,
                FocusArea = dto.FocusArea,
                ProgramType = dto.ProgramType,
                WeeklyFrequency = dto.WeeklyFrequency,
                EstimatedDuration = dto.EstimatedDuration,
                Equipment = JsonSerializer.Serialize(dto.Equipment),
                TargetMuscles = JsonSerializer.Serialize(dto.TargetMuscles),
                ProgramOverview = dto.ProgramOverview,
                Benefits = JsonSerializer.Serialize(dto.Benefits),
                Tips = JsonSerializer.Serialize(dto.Tips),
                CreatedAt = DateTime.UtcNow
            };
            _context.WorkoutPrograms.Add(w);
            await _context.SaveChangesAsync();

            dto.Id = w.Id;
            return CreatedAtAction(nameof(GetById), new { id = w.Id }, dto);
        }

        // PUT: api/workoutprograms/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] WorkoutProgramDto dto)
        {
            var w = await _context.WorkoutPrograms.FindAsync(id);
            if (w == null) return NotFound();

            w.Title = dto.Title;
            w.ImageUrl = dto.ImageUrl;
            w.Duration = dto.Duration;
            w.Difficulty = dto.Difficulty;
            w.FocusArea = dto.FocusArea;
            w.ProgramType = dto.ProgramType;
            w.WeeklyFrequency = dto.WeeklyFrequency;
            w.EstimatedDuration = dto.EstimatedDuration;
            w.Equipment = JsonSerializer.Serialize(dto.Equipment);
            w.TargetMuscles = JsonSerializer.Serialize(dto.TargetMuscles);
            w.ProgramOverview = dto.ProgramOverview;
            w.Benefits = JsonSerializer.Serialize(dto.Benefits);
            w.Tips = JsonSerializer.Serialize(dto.Tips);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/workoutprograms/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var w = await _context.WorkoutPrograms.FindAsync(id);
            if (w == null) return NotFound();

            _context.WorkoutPrograms.Remove(w);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}


