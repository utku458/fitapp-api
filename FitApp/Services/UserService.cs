using FitApp.DTOs;
using FitApp.Data;
using FitApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task UpdateUserDetails(int userId, UserDetailsDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            // Check if UserDetails exists for this user
            var userDetails = await _context.UserDetails.FirstOrDefaultAsync(d => d.UserId == userId);
            if (userDetails == null)
            {
                userDetails = new UserDetails { UserId = userId };
                _context.UserDetails.Add(userDetails);
            }

            userDetails.FitnessGoal = dto.FitnessGoal;
            userDetails.Gender = dto.Gender;
            userDetails.Age = dto.Age;
            userDetails.Height = dto.Height;
            userDetails.Weight = dto.Weight;
            userDetails.HasTrainingExperience = dto.HasTrainingExperience;
            userDetails.FitnessLevel = dto.FitnessLevel;
            userDetails.DailyCalorieGoal = dto.DailyCalorieGoal;
            userDetails.DailyWaterGoal = dto.DailyWaterGoal;
            userDetails.DailyStepGoal = dto.DailyStepGoal;
            userDetails.WeeklyCommitment = dto.WeeklyCommitment;

            await _context.SaveChangesAsync();
        }
    }
} 