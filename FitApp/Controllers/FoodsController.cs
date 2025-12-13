using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitApp.Data;
using FitApp.DTOs;
using FitApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace FitApp.Controllersdotdotn
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FoodsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/foods?query=&page=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult> GetFoods([FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var foodsQuery = _context.FoodItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                foodsQuery = foodsQuery.Where(f => f.Name.Contains(query));
            }

            var totalCount = await foodsQuery.CountAsync();
            var foods = await foodsQuery
                .OrderBy(f => f.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FoodItemDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    CaloriesPer100g = f.CaloriesPer100g,
                    ProteinPer100g = f.ProteinPer100g,
                    CarbsPer100g = f.CarbsPer100g,
                    FatPer100g = f.FatPer100g,
                    Locale = f.Locale,
                    Brand = f.Brand,
                    Category = f.Category,
                    PortionName = f.PortionName,
                    PortionGrams = f.PortionGrams
                })
                .ToListAsync();

            return Ok(new
            {
                foods,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        // GET: api/foods/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<FoodItemDto>> GetFood(int id)
        {
            var food = await _context.FoodItems.FindAsync(id);
            if (food == null)
                return NotFound();

            var dto = new FoodItemDto
            {
                Id = food.Id,
                Name = food.Name,
                CaloriesPer100g = food.CaloriesPer100g,
                ProteinPer100g = food.ProteinPer100g,
                CarbsPer100g = food.CarbsPer100g,
                FatPer100g = food.FatPer100g,
                Locale = food.Locale,
                Brand = food.Brand,
                Category = food.Category,
                PortionName = food.PortionName,
                PortionGrams = food.PortionGrams
            };

            return Ok(dto);
        }

        // POST: api/foods (Admin only)
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<FoodItemDto>> CreateFood([FromBody] FoodItemDto dto)
        {
            var food = new FoodItem
            {
                Name = dto.Name,
                CaloriesPer100g = dto.CaloriesPer100g,
                ProteinPer100g = dto.ProteinPer100g,
                CarbsPer100g = dto.CarbsPer100g,
                FatPer100g = dto.FatPer100g,
                Locale = dto.Locale ?? "tr-TR",
                Brand = dto.Brand,
                Category = dto.Category,
                PortionName = dto.PortionName,
                PortionGrams = dto.PortionGrams
            };

            _context.FoodItems.Add(food);
            await _context.SaveChangesAsync();

            dto.Id = food.Id;
            return CreatedAtAction(nameof(GetFood), new { id = food.Id }, dto);
        }

        // PUT: api/foods/{id} (Admin only)
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateFood(int id, [FromBody] FoodItemDto dto)
        {
            var food = await _context.FoodItems.FindAsync(id);
            if (food == null)
                return NotFound();

            food.Name = dto.Name;
            food.CaloriesPer100g = dto.CaloriesPer100g;
            food.ProteinPer100g = dto.ProteinPer100g;
            food.CarbsPer100g = dto.CarbsPer100g;
            food.FatPer100g = dto.FatPer100g;
            food.Locale = dto.Locale ?? "tr-TR";
            food.Brand = dto.Brand;
            food.Category = dto.Category;
            food.PortionName = dto.PortionName;
            food.PortionGrams = dto.PortionGrams;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/foods/{id} (Admin only)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteFood(int id)
        {
            var food = await _context.FoodItems.FindAsync(id);
            if (food == null)
                return NotFound();

            _context.FoodItems.Remove(food);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}


