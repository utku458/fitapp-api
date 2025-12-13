using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FitApp.Data;
using FitApp.Models;
using FitApp.DTOs;
using FitApp.Services;

namespace FitApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TodosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStreakService _streakService;

        public TodosController(ApplicationDbContext context, ICurrentUserService currentUserService, IStreakService streakService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _streakService = streakService;
        }

        // GET: api/todos?date=YYYY-MM-DD
        [HttpGet]
        public async Task<ActionResult<TodoListResponse>> GetTodos(
            [FromQuery] string? category,
            [FromQuery] string? from,
            [FromQuery] string? to,
            [FromQuery] bool? onlyActive,
            [FromQuery] string? date,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "createdAt",
            [FromQuery] string? sortOrder = "desc")
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                    return Unauthorized(new { message = "Invalid token", statusCode = 401, timestamp = DateTime.UtcNow });

                var targetDate = DateTime.TryParse(date, out var parsedDate) ? parsedDate : DateTime.Today;

                var query = _context.Todos.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(category) && Enum.TryParse<TodoCategory>(category, true, out var categoryEnum))
                {
                    query = query.Where(t => t.Category == categoryEnum);
                }

                // Date range filter
                if (!string.IsNullOrEmpty(from) && DateTime.TryParse(from, out var fromDate))
                {
                    query = query.Where(t => t.StartDate == null || t.StartDate <= fromDate);
                }

                if (!string.IsNullOrEmpty(to) && DateTime.TryParse(to, out var toDate))
                {
                    query = query.Where(t => t.EndDate == null || t.EndDate >= toDate);
                }

                // Active filter - sadece belirli tarihte aktif olan todo'lar
                if (onlyActive == true && !string.IsNullOrEmpty(from))
                {
                    var activeTargetDate = DateTime.TryParse(from, out var activeParsedDate) ? activeParsedDate : DateTime.Today;
                    query = query.Where(t => IsTodoActiveForDate(t, activeTargetDate));
                }

                // Apply sorting
                query = sortBy?.ToLower() switch
                {
                    "title" => sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(t => t.Title)
                        : query.OrderBy(t => t.Title),
                    "startdate" => sortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.StartDate)
                        : query.OrderBy(t => t.StartDate),
                    _ => sortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.CreatedAt)
                        : query.OrderBy(t => t.CreatedAt)
                };

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var todos = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TodoDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        Title = t.Title,
                        Description = t.Description,
                        Category = t.Category,
                        TaskType = t.TaskType,
                        RepeatDays = t.RepeatDays,
                        IntervalDays = t.IntervalDays,
                        Streak = t.Streak,
                        IsCompleted = t.IsCompleted,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        TargetDate = t.TargetDate,
                        ProgressPercentage = t.ProgressPercentage,
                        StartingValue = t.StartingValue,
                        TargetValue = t.TargetValue,
                        IsArchived = t.IsArchived,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        IsCompletedForDate = t.IsCompleted, // Todo'nun genel tamamlanma durumu
                        CompletedAtForDate = t.IsCompleted ? t.UpdatedAt : null // Tamamlandıysa UpdatedAt, değilse null
                    })
                    .ToListAsync();

                var response = new TodoListResponse
                {
                    Todos = todos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", statusCode = 500, timestamp = DateTime.UtcNow });
            }
        }

        // GET: api/todos/active?date=YYYY-MM-DD
        [HttpGet("active")]
        public async Task<ActionResult<ActiveTodosResponse>> GetActiveTodos([FromQuery] string? date)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                    return Unauthorized(new { message = "Invalid token", statusCode = 401, timestamp = DateTime.UtcNow });

                var targetDate = DateTime.TryParse(date, out var parsedDate) ? parsedDate : DateTime.Today;

                var activeTodos = await _context.Todos
                    .Where(t => IsTodoActiveForDate(t, targetDate))
                    .Select(t => new TodoDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        Title = t.Title,
                        Description = t.Description,
                        Category = t.Category,
                        TaskType = t.TaskType,
                        RepeatDays = t.RepeatDays,
                        IntervalDays = t.IntervalDays,
                        Streak = t.Streak,
                        IsCompleted = t.IsCompleted,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        TargetDate = t.TargetDate,
                        ProgressPercentage = t.ProgressPercentage,
                        StartingValue = t.StartingValue,
                        TargetValue = t.TargetValue,
                        IsArchived = t.IsArchived,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        IsCompletedForDate = t.IsCompleted, // Todo'nun genel tamamlanma durumu
                        CompletedAtForDate = t.IsCompleted ? t.UpdatedAt : null // Tamamlandıysa UpdatedAt, değilse null
                    })
                    .ToListAsync();

                var response = new ActiveTodosResponse
                {
                    ActiveTodos = activeTodos,
                    Date = targetDate,
                    TotalCount = activeTodos.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", statusCode = 500, timestamp = DateTime.UtcNow });
            }
        }

        // GET: api/todos/{id}?date=YYYY-MM-DD
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoDto>> GetTodo(int id, [FromQuery] string? date)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                    return Unauthorized(new { message = "Invalid token", statusCode = 401, timestamp = DateTime.UtcNow });

                var targetDate = DateTime.TryParse(date, out var parsedDate) ? parsedDate : DateTime.Today;

                var todo = await _context.Todos
                    .Where(t => t.Id == id)
                    .Select(t => new TodoDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        Title = t.Title,
                        Description = t.Description,
                        Category = t.Category,
                        TaskType = t.TaskType,
                        RepeatDays = t.RepeatDays,
                        IntervalDays = t.IntervalDays,
                        Streak = t.Streak,
                        IsCompleted = t.IsCompleted,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        TargetDate = t.TargetDate,
                        ProgressPercentage = t.ProgressPercentage,
                        StartingValue = t.StartingValue,
                        TargetValue = t.TargetValue,
                        IsArchived = t.IsArchived,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        IsCompletedForDate = t.IsCompleted, // Todo'nun genel tamamlanma durumu
                        CompletedAtForDate = t.IsCompleted ? t.UpdatedAt : null // Tamamlandıysa UpdatedAt, değilse null
                    })
                    .FirstOrDefaultAsync();

                if (todo == null)
                    return NotFound(new { message = "Todo not found", statusCode = 404, timestamp = DateTime.UtcNow });

                return Ok(todo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", statusCode = 500, timestamp = DateTime.UtcNow });
            }
        }

        // POST: api/todos
        [HttpPost]
        public async Task<ActionResult<TodoDto>> CreateTodo(CreateTodoDto createTodoDto)
        {
            try
            {
                // 🔍 DEBUG: Request'i logla
                Console.WriteLine($"🔍 DEBUG: ===== CREATE TODO REQUEST =====");
                Console.WriteLine($"🔍 DEBUG: Title: {createTodoDto.Title}");
                Console.WriteLine($"🔍 DEBUG: TaskType: {createTodoDto.TaskType} ({(int)createTodoDto.TaskType})");
                Console.WriteLine($"🔍 DEBUG: Category: {createTodoDto.Category} ({(int)createTodoDto.Category})");
                Console.WriteLine($"🔍 DEBUG: CategoryType: {createTodoDto.CategoryType} ({(int?)createTodoDto.CategoryType})");
                Console.WriteLine($"🔍 DEBUG: RepeatDays: {createTodoDto.RepeatDays}");
                Console.WriteLine($"🔍 DEBUG: IntervalDays: {createTodoDto.IntervalDays}");
                Console.WriteLine($"🔍 DEBUG: StartDate: {createTodoDto.StartDate}");
                Console.WriteLine($"🔍 DEBUG: EndDate: {createTodoDto.EndDate}");
                Console.WriteLine($"🔍 DEBUG: ================================");
                
                // 🔍 DEBUG: Raw request body'yi logla
                Request.EnableBuffering();
                Request.Body.Position = 0;
                using var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
                Console.WriteLine($"🔍 DEBUG: ===== RAW REQUEST BODY =====");
                Console.WriteLine($"🔍 DEBUG: {requestBody}");
                Console.WriteLine($"🔍 DEBUG: ==============================");
                
                // 🔍 DEBUG: TaskType kullanılıyor
                Console.WriteLine($"🔍 DEBUG: ===== USING TASKTYPE =====");
                Console.WriteLine($"🔍 DEBUG: TaskType: {createTodoDto.TaskType} ({(int)createTodoDto.TaskType})");
                Console.WriteLine($"🔍 DEBUG: ==========================");

                var userId = _currentUserService.UserId;
                if (userId == null)
                    return Unauthorized(new { message = "Invalid token", statusCode = 401, timestamp = DateTime.UtcNow });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { message = "Validation failed", errors, statusCode = 400, timestamp = DateTime.UtcNow });
                }

                // Validasyon
                if (createTodoDto.TaskType == TaskType.Weekly && createTodoDto.RepeatDays == 0)
                {
                    return BadRequest(new { message = "Weekly todos must have at least one weekday selected", statusCode = 400, timestamp = DateTime.UtcNow });
                }
                
                if (createTodoDto.TaskType == TaskType.Monthly && (createTodoDto.StartDate == null || createTodoDto.EndDate == null))
                {
                    return BadRequest(new { message = "Monthly todos must have both start and end dates", statusCode = 400, timestamp = DateTime.UtcNow });
                }

                var todo = new Todo
                {
                    UserId = userId.Value,
                    Title = createTodoDto.Title,
                    Description = createTodoDto.Description,
                    Category = createTodoDto.CategoryType ?? createTodoDto.Category, // CategoryType varsa onu kullan, yoksa Category'yi kullan
                    TaskType = createTodoDto.TaskType, // TaskType varsa onu kullan, yoksa RecurrenceType'ı kullan
                    RepeatDays = createTodoDto.RepeatDays,
                    IntervalDays = createTodoDto.IntervalDays,
                    StartDate = createTodoDto.StartDate,
                    EndDate = createTodoDto.EndDate,
                    TargetDate = createTodoDto.TargetDate,
                    StartingValue = createTodoDto.StartingValue,
                    TargetValue = createTodoDto.TargetValue,
                    ProgressPercentage = 0,
                    IsArchived = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // 🔍 DEBUG: Todo object'i logla
                Console.WriteLine($"🔍 DEBUG: ===== TODO OBJECT BEFORE SAVE =====");
                Console.WriteLine($"🔍 DEBUG: Todo RecurrenceType: {todo.TaskType} ({(int)todo.TaskType})");
                Console.WriteLine($"🔍 DEBUG: Todo Category: {todo.Category} ({(int)todo.Category})");
                Console.WriteLine($"🔍 DEBUG: Todo WeekdaysMask: {todo.RepeatDays}");
                Console.WriteLine($"🔍 DEBUG: Todo StartDate: {todo.StartDate}");
                Console.WriteLine($"🔍 DEBUG: Todo EndDate: {todo.EndDate}");
                Console.WriteLine($"🔍 DEBUG: ======================================");

                _context.Todos.Add(todo);
                await _context.SaveChangesAsync();

                // 🔍 DEBUG: Save sonrası logla
                Console.WriteLine($"🔍 DEBUG: ===== TODO OBJECT AFTER SAVE =====");
                Console.WriteLine($"🔍 DEBUG: Todo ID: {todo.Id}");
                Console.WriteLine($"🔍 DEBUG: Todo RecurrenceType: {todo.TaskType} ({(int)todo.TaskType})");
                Console.WriteLine($"🔍 DEBUG: Todo Category: {todo.Category} ({(int)todo.Category})");
                Console.WriteLine($"🔍 DEBUG: Todo WeekdaysMask: {todo.RepeatDays}");
                Console.WriteLine($"🔍 DEBUG: Todo StartDate: {todo.StartDate}");
                Console.WriteLine($"🔍 DEBUG: Todo EndDate: {todo.EndDate}");
                Console.WriteLine($"🔍 DEBUG: =====================================");

                var todoDto = new TodoDto
                {
                    Id = todo.Id,
                    UserId = todo.UserId,
                    Title = todo.Title,
                    Description = todo.Description,
                    Category = todo.Category,
                        TaskType = todo.TaskType,
                        RepeatDays = todo.RepeatDays,
                        IntervalDays = todo.IntervalDays,
                        Streak = todo.Streak,
                        IsCompleted = todo.IsCompleted,
                    StartDate = todo.StartDate,
                    EndDate = todo.EndDate,
                    TargetDate = todo.TargetDate,
                    ProgressPercentage = todo.ProgressPercentage,
                    StartingValue = todo.StartingValue,
                    TargetValue = todo.TargetValue,
                    IsArchived = todo.IsArchived,
                    CreatedAt = todo.CreatedAt,
                    UpdatedAt = todo.UpdatedAt,
                    IsCompletedForDate = todo.IsCompleted, // Todo'nun genel tamamlanma durumu
                    CompletedAtForDate = todo.IsCompleted ? todo.UpdatedAt : null // Tamamlandıysa UpdatedAt, değilse null
                };

                // 🔍 DEBUG: Response DTO'yu logla
                Console.WriteLine($"🔍 DEBUG: ===== RESPONSE DTO =====");
                Console.WriteLine($"🔍 DEBUG: Response RecurrenceType: {todoDto.TaskType} ({(int)todoDto.TaskType})");
                Console.WriteLine($"🔍 DEBUG: Response Category: {todoDto.Category} ({(int)todoDto.Category})");
                Console.WriteLine($"🔍 DEBUG: Response WeekdaysMask: {todoDto.RepeatDays}");
                Console.WriteLine($"🔍 DEBUG: Response StartDate: {todoDto.StartDate}");
                Console.WriteLine($"🔍 DEBUG: Response EndDate: {todoDto.EndDate}");
                Console.WriteLine($"🔍 DEBUG: =========================");

                return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", statusCode = 500, timestamp = DateTime.UtcNow });
            }
        }

        // PUT: api/todos/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<TodoDto>> UpdateTodo(int id, UpdateTodoDto updateTodoDto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                    return Unauthorized(new { message = "Invalid token", statusCode = 401, timestamp = DateTime.UtcNow });

                var todo = await _context.Todos
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (todo == null)
                    return NotFound(new { message = "Todo not found", statusCode = 404, timestamp = DateTime.UtcNow });

                // Update only provided fields
                if (!string.IsNullOrEmpty(updateTodoDto.Title))
                    todo.Title = updateTodoDto.Title;

                if (updateTodoDto.Description != null)
                    todo.Description = updateTodoDto.Description;

                if (updateTodoDto.Category.HasValue)
                    todo.Category = updateTodoDto.Category.Value;
                
                // CategoryType varsa Category'yi override et
                if (updateTodoDto.CategoryType.HasValue)
                    todo.Category = updateTodoDto.CategoryType.Value;

                if (updateTodoDto.TaskType.HasValue)
                    todo.TaskType = updateTodoDto.TaskType.Value;
                
                // TaskType zaten yukarıda set edildi

                if (updateTodoDto.RepeatDays.HasValue)
                    todo.RepeatDays = updateTodoDto.RepeatDays.Value;
                    
                if (updateTodoDto.IntervalDays.HasValue)
                    todo.IntervalDays = updateTodoDto.IntervalDays.Value;

                if (updateTodoDto.StartDate.HasValue)
                    todo.StartDate = updateTodoDto.StartDate.Value;

                if (updateTodoDto.EndDate.HasValue)
                    todo.EndDate = updateTodoDto.EndDate.Value;

                if (updateTodoDto.TargetDate.HasValue)
                    todo.TargetDate = updateTodoDto.TargetDate.Value;

                if (!string.IsNullOrEmpty(updateTodoDto.StartingValue))
                    todo.StartingValue = updateTodoDto.StartingValue;

                if (!string.IsNullOrEmpty(updateTodoDto.TargetValue))
                    todo.TargetValue = updateTodoDto.TargetValue;

                todo.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var todoDto = new TodoDto
                {
                    Id = todo.Id,
                    UserId = todo.UserId,
                    Title = todo.Title,
                    Description = todo.Description,
                    Category = todo.Category,
                        TaskType = todo.TaskType,
                        RepeatDays = todo.RepeatDays,
                        IntervalDays = todo.IntervalDays,
                        Streak = todo.Streak,
                        IsCompleted = todo.IsCompleted,
                    StartDate = todo.StartDate,
                    EndDate = todo.EndDate,
                    TargetDate = todo.TargetDate,
                    ProgressPercentage = todo.ProgressPercentage,
                    StartingValue = todo.StartingValue,
                    TargetValue = todo.TargetValue,
                    IsArchived = todo.IsArchived,
                    CreatedAt = todo.CreatedAt,
                    UpdatedAt = todo.UpdatedAt,
                    IsCompletedForDate = todo.IsCompleted, // Todo'nun genel tamamlanma durumu
                    CompletedAtForDate = todo.IsCompleted ? todo.UpdatedAt : null // Tamamlandıysa UpdatedAt, değilse null
                };

                return Ok(todoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", statusCode = 500, timestamp = DateTime.UtcNow });
            }
        }

        // DELETE: api/todos/{id} - Soft delete (archive)
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodo(int id)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                    return Unauthorized(new { message = "Invalid token", statusCode = 401, timestamp = DateTime.UtcNow });

                var todo = await _context.Todos
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (todo == null)
                    return NotFound(new { message = "Todo not found", statusCode = 404, timestamp = DateTime.UtcNow });

                // Soft delete - archive instead of hard delete
                todo.IsArchived = true;
                todo.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Todo archived successfully", statusCode = 200, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", statusCode = 500, timestamp = DateTime.UtcNow });
            }
        }

        // POST: api/todos/{id}/complete
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<TodoCompletionDto>> CompleteTodo(int id, CompleteTodoDto completeTodoDto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                    return Unauthorized(new { message = "Invalid token", statusCode = 401, timestamp = DateTime.UtcNow });

                var todo = await _context.Todos
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (todo == null)
                    return NotFound(new { message = "Todo not found", statusCode = 404, timestamp = DateTime.UtcNow });

                // Todo'yu tamamlandı olarak işaretle
                todo.IsCompleted = true;
                todo.UpdatedAt = DateTime.UtcNow;

                // TaskCompletion record oluştur
                var completionDate = completeTodoDto.Date.Date;
                
                var existingCompletion = await _context.TaskCompletions
                    .FirstOrDefaultAsync(tc => tc.TodoId == id && tc.CompletionDate.Date == completionDate);

                if (existingCompletion == null)
                {
                    var taskCompletion = new TaskCompletion
                    {
                        TodoId = id,
                        CompletionDate = completionDate,
                        CompletedAt = DateTime.UtcNow
                    };

                    _context.TaskCompletions.Add(taskCompletion);
                }

                await _context.SaveChangesAsync();

                // Streak'i güncelle
                await _streakService.CompleteTaskAsync(id, completionDate);

                return Ok(new { message = "Todo completed successfully", statusCode = 200, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", statusCode = 500, timestamp = DateTime.UtcNow });
            }
        }

        // DELETE: api/todos/{id}/complete?date=YYYY-MM-DD - Tamamlanma kaydını sil
        [HttpDelete("{id}/complete")]
        public async Task<ActionResult> UncompleteTodo(int id, [FromQuery] string date)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                    return Unauthorized(new { message = "Invalid token", statusCode = 401, timestamp = DateTime.UtcNow });

                if (!DateTime.TryParse(date, out var completionDate))
                {
                    return BadRequest(new { message = "Invalid date format", statusCode = 400, timestamp = DateTime.UtcNow });
                }

                var todo = await _context.Todos
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (todo == null)
                    return NotFound(new { message = "Todo not found", statusCode = 404, timestamp = DateTime.UtcNow });

                // Todo'yu tamamlanmamış olarak işaretle
                todo.IsCompleted = false;
                todo.UpdatedAt = DateTime.UtcNow;

                // TaskCompletion record'u sil
                var taskCompletion = await _context.TaskCompletions
                    .FirstOrDefaultAsync(tc => tc.TodoId == id && tc.CompletionDate.Date == completionDate.Date);

                if (taskCompletion != null)
                {
                    _context.TaskCompletions.Remove(taskCompletion);
                }

                await _context.SaveChangesAsync();

                // Streak'i güncelle
                await _streakService.UncompleteTaskAsync(id, completionDate);

                return Ok(new { message = "Todo uncompleted successfully", statusCode = 200, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", statusCode = 500, timestamp = DateTime.UtcNow });
            }
        }

        // PUT: api/todos/{id}/progress - Hedef tabanlı todo'lar için ilerleme güncelleme
        [HttpPut("{id}/progress")]
        public async Task<ActionResult<TodoDto>> UpdateProgress(int id, UpdateProgressDto updateProgressDto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                    return Unauthorized(new { message = "Invalid token", statusCode = 401, timestamp = DateTime.UtcNow });

                var todo = await _context.Todos
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (todo == null)
                    return NotFound(new { message = "Todo not found", statusCode = 404, timestamp = DateTime.UtcNow });

                if (todo.TaskType != TaskType.Goal)
                {
                    return BadRequest(new { message = "Progress can only be updated for goal-based todos", statusCode = 400, timestamp = DateTime.UtcNow });
                }

                todo.ProgressPercentage = updateProgressDto.ProgressPercentage;
                todo.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var todoDto = new TodoDto
                {
                    Id = todo.Id,
                    UserId = todo.UserId,
                    Title = todo.Title,
                    Description = todo.Description,
                    Category = todo.Category,
                        TaskType = todo.TaskType,
                        RepeatDays = todo.RepeatDays,
                        IntervalDays = todo.IntervalDays,
                        Streak = todo.Streak,
                        IsCompleted = todo.IsCompleted,
                    StartDate = todo.StartDate,
                    EndDate = todo.EndDate,
                    TargetDate = todo.TargetDate,
                    ProgressPercentage = todo.ProgressPercentage,
                    StartingValue = todo.StartingValue,
                    TargetValue = todo.TargetValue,
                    IsArchived = todo.IsArchived,
                    CreatedAt = todo.CreatedAt,
                    UpdatedAt = todo.UpdatedAt,
                    IsCompletedForDate = todo.IsCompleted, // Todo'nun genel tamamlanma durumu
                    CompletedAtForDate = todo.IsCompleted ? todo.UpdatedAt : null // Tamamlandıysa UpdatedAt, değilse null
                };

                return Ok(todoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", statusCode = 500, timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Belirli bir tarihte todo'nun aktif olup olmadığını kontrol eder
        /// </summary>
        private static bool IsTodoActiveForDate(Todo todo, DateTime date)
        {
            // Arşivlenmiş todo'lar aktif değil
            if (todo.IsArchived)
                return false;

            // Başlangıç tarihi kontrolü
            if (todo.StartDate.HasValue && todo.StartDate > date)
                return false;

            // Bitiş tarihi kontrolü
            if (todo.EndDate.HasValue && todo.EndDate < date)
                return false;

            // Günlük todo'lar için - her gün aktif
            if (todo.TaskType == TaskType.Daily)
            {
                return true;
            }

            // Haftalık todo'lar için
            if (todo.TaskType == TaskType.Weekly)
            {
                var dayOfWeek = GetDayOfWeekBit(date.DayOfWeek);
                return (todo.RepeatDays & dayOfWeek) != 0;
            }

            // Aylık todo'lar için - startDate ve endDate arasında aktif
            if (todo.TaskType == TaskType.Monthly)
            {
                return todo.StartDate?.Date <= date.Date && todo.EndDate?.Date >= date.Date;
            }

            // Hedef tabanlı todo'lar için - hedef tarihine kadar aktif
            if (todo.TaskType == TaskType.Goal)
            {
                return todo.TargetDate?.Date >= date.Date;
            }

            return false;
        }

        /// <summary>
        /// DateTime.DayOfWeek'i bitmask'e çevirir
        /// </summary>
        private static int GetDayOfWeekBit(DayOfWeek dayOfWeek)
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
    }
}