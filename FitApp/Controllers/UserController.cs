using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitApp.Data;
using FitApp.Models;
using System.Security.Claims;
using FitApp.DTOs;
using FitApp.Services;

namespace FitApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserService _userService;

        public UserController(ApplicationDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<object>> GetProfile()
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Invalid token");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid user ID in token");
            }

            // Find user in database
            var user = await _context.Users
                .Select(u => new { u.Id, u.Email })
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }

        [HttpGet("details/{id?}")]
        public async Task<ActionResult<UserDataDto>> GetUserDetails(int? id = null)
        {
            int userId;
            if (id.HasValue)
            {
                userId = id.Value;
            }
            else
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out userId))
                {
                    return Unauthorized("Invalid token");
                }
            }

            var user = await _context.Users
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found");

            var details = user.UserDetails;
            var dto = new UserDataDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Password = user.PasswordHash, // Not: Hashlenmiş hali, güvenlik için normalde dönülmez
                FitnessGoal = details?.FitnessGoal,
                Gender = details?.Gender,
                Weight = details?.Weight,
                Age = details?.Age,
                Height = details?.Height,
                HasTrainingExperience = details?.HasTrainingExperience,
                FitnessLevel = details?.FitnessLevel,
                DailyCalorieGoal = details?.DailyCalorieGoal,
                DailyWaterGoal = details?.DailyWaterGoal,
                DailyStepGoal = details?.DailyStepGoal,
                WeeklyCommitment = details?.WeeklyCommitment
            };
            return Ok(dto);
        }

        [HttpPost("details")]
        public async Task<IActionResult> CompleteProfile([FromBody] UserDetailsDto detailsDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _userService.UpdateUserDetails(userId, detailsDto);
            return Ok();
        }
    }
} 