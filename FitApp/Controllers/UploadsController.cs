using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        public UploadsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("image")]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("file");
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
            if (!allowed.Contains(ext)) return BadRequest("type");
            const long maxSize = 5L * 1024 * 1024; // 5 MB
            if (file.Length > maxSize) return BadRequest("size");

            var name = $"{Guid.NewGuid()}{ext}";
            var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "articles");
            Directory.CreateDirectory(uploadsDir);
            var savePath = Path.Combine(uploadsDir, name);

            using (var fs = System.IO.File.Create(savePath))
            {
                await file.CopyToAsync(fs);
            }

            var url = $"{Request.Scheme}://{Request.Host}/uploads/articles/{name}";
            return Ok(new { url });
        }
    }
}

