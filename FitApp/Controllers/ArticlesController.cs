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
    public class ArticlesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ArticlesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetArticles(
            [FromQuery] string? query, 
            [FromQuery] string? category, 
            [FromQuery] string? difficulty,
            [FromQuery] bool featured = false,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            var q = _context.Articles
                .Include(a => a.Sections)
                .Include(a => a.Images)
                .Include(a => a.Tips)
                .Include(a => a.Warnings)
                .Include(a => a.KeyTakeaways)
                .Include(a => a.References)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var like = query.Trim();
                q = q.Where(a => a.Title.Contains(like) || a.Description.Contains(like) || a.Content.Contains(like));
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                q = q.Where(a => a.Category == category);
            }
            if (!string.IsNullOrWhiteSpace(difficulty))
            {
                q = q.Where(a => a.DifficultyLevel == difficulty);
            }
            if (featured)
            {
                q = q.Where(a => a.IsFeatured);
            }

            var total = await q.CountAsync();
            var items = await q
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = items.Select(a => new EnhancedArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Author = a.Author,
                Category = a.Category,
                ImageUrl = a.ImageUrl,
                EstimatedReadMinutes = a.EstimatedReadMinutes,
                CreatedAt = a.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Content = a.Content,
                DifficultyLevel = a.DifficultyLevel,
                Tags = !string.IsNullOrEmpty(a.Tags) ? JsonSerializer.Deserialize<List<string>>(a.Tags) : new List<string>(),
                RelatedArticles = !string.IsNullOrEmpty(a.RelatedArticles) ? JsonSerializer.Deserialize<List<int>>(a.RelatedArticles) : new List<int>(),
                ViewCount = a.ViewCount,
                Rating = a.Rating,
                IsFeatured = a.IsFeatured,
                Sections = a.Sections
                    .OrderBy(s => s.SectionOrder)
                    .Select(s => new ArticleSectionDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Content = s.Content,
                        SectionOrder = s.SectionOrder,
                        Images = s.Images.OrderBy(i => i.ImageOrder).Select(i => i.ImageUrl).ToList(),
                        Tips = s.Tips.OrderBy(t => t.TipOrder).Select(t => t.TipText).ToList(),
                        Warnings = s.Warnings.OrderBy(w => w.WarningOrder).Select(w => w.WarningText).ToList()
                    }).ToList(),
                KeyTakeaways = a.KeyTakeaways.OrderBy(kt => kt.TakeawayOrder).Select(kt => kt.TakeawayText).ToList(),
                References = a.References
                    .OrderBy(r => r.ReferenceOrder)
                    .Select(r => new ArticleReferenceDto
                    {
                        Id = r.Id,
                        ReferenceText = r.ReferenceText,
                        ReferenceUrl = r.ReferenceUrl,
                        ReferenceOrder = r.ReferenceOrder
                    }).ToList()
            }).ToList();

            return Ok(new { articles = result, totalCount = total, page, pageSize, totalPages = (int)Math.Ceiling((double)total / pageSize) });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var a = await _context.Articles
                .Include(a => a.Sections)
                    .ThenInclude(s => s.Images)
                .Include(a => a.Sections)
                    .ThenInclude(s => s.Tips)
                .Include(a => a.Sections)
                    .ThenInclude(s => s.Warnings)
                .Include(a => a.Images)
                .Include(a => a.Tips)
                .Include(a => a.Warnings)
                .Include(a => a.KeyTakeaways)
                .Include(a => a.References)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (a == null) return NotFound();

            // Increment view count
            a.ViewCount++;
            await _context.SaveChangesAsync();

            var result = new EnhancedArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Author = a.Author,
                Category = a.Category,
                ImageUrl = a.ImageUrl,
                EstimatedReadMinutes = a.EstimatedReadMinutes,
                CreatedAt = a.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Content = a.Content,
                DifficultyLevel = a.DifficultyLevel,
                Tags = !string.IsNullOrEmpty(a.Tags) ? JsonSerializer.Deserialize<List<string>>(a.Tags) : new List<string>(),
                RelatedArticles = !string.IsNullOrEmpty(a.RelatedArticles) ? JsonSerializer.Deserialize<List<int>>(a.RelatedArticles) : new List<int>(),
                ViewCount = a.ViewCount,
                Rating = a.Rating,
                IsFeatured = a.IsFeatured,
                Sections = a.Sections
                    .OrderBy(s => s.SectionOrder)
                    .Select(s => new ArticleSectionDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Content = s.Content,
                        SectionOrder = s.SectionOrder,
                        Images = s.Images.OrderBy(i => i.ImageOrder).Select(i => i.ImageUrl).ToList(),
                        Tips = s.Tips.OrderBy(t => t.TipOrder).Select(t => t.TipText).ToList(),
                        Warnings = s.Warnings.OrderBy(w => w.WarningOrder).Select(w => w.WarningText).ToList()
                    }).ToList(),
                KeyTakeaways = a.KeyTakeaways.OrderBy(kt => kt.TakeawayOrder).Select(kt => kt.TakeawayText).ToList(),
                References = a.References
                    .OrderBy(r => r.ReferenceOrder)
                    .Select(r => new ArticleReferenceDto
                    {
                        Id = r.Id,
                        ReferenceText = r.ReferenceText,
                        ReferenceUrl = r.ReferenceUrl,
                        ReferenceOrder = r.ReferenceOrder
                    }).ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ArticleCreateDto dto)
        {
            var a = new Article
            {
                Title = dto.Title,
                Description = dto.Description,
                Author = dto.Author,
                Category = dto.Category,
                ImageUrl = dto.ImageUrl,
                EstimatedReadMinutes = dto.EstimatedReadMinutes,
                CreatedAt = DateTime.UtcNow
            };
            _context.Articles.Add(a);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = a.Id }, new ArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Author = a.Author,
                Category = a.Category,
                ImageUrl = a.ImageUrl,
                EstimatedReadMinutes = a.EstimatedReadMinutes,
                CreatedAt = a.CreatedAt
            });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ArticleCreateDto dto)
        {
            var a = await _context.Articles.FindAsync(id);
            if (a == null) return NotFound();
            a.Title = dto.Title;
            a.Description = dto.Description;
            a.Author = dto.Author;
            a.Category = dto.Category;
            a.ImageUrl = dto.ImageUrl;
            a.EstimatedReadMinutes = dto.EstimatedReadMinutes;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/rate")]
        public async Task<IActionResult> RateArticle(int id, [FromBody] RatingDto rating)
        {
            var a = await _context.Articles.FindAsync(id);
            if (a == null) return NotFound();

            if (rating.Rating < 0 || rating.Rating > 5)
            {
                return BadRequest("Rating must be between 0 and 5");
            }

            // Simple average rating calculation
            // In a real app, you might want to store individual user ratings
            a.Rating = (a.Rating + rating.Rating) / 2;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rating updated successfully", newRating = a.Rating });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var a = await _context.Articles.FindAsync(id);
            if (a == null) return NotFound();
            _context.Articles.Remove(a);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

