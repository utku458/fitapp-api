using System.Transactions;
using FitApp.Data;
using FitApp.DTOs;
using FitApp.Helpers;
using FitApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthService(ApplicationDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        public async Task<User> Register(RegisterDto registerDto)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted
            };
            using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                    {
                        throw new Exception("User already exists");
                    }

                    // Hash password with BCrypt
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password, workFactor: 12);

                    var user = new User
                    {
                        Email = registerDto.Email,
                        PasswordHash = passwordHash,
                        Name = registerDto.Name
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    var userDetails = new UserDetails { UserId = user.Id };
                    _context.UserDetails.Add(userDetails);
                    await _context.SaveChangesAsync();

                    scope.Complete();
                    return user;
                }
                catch (Exception)
                {
                    // Transaction will be rolled back automatically if not completed
                    throw;
                }
            }
        }

        public async Task<LoginResponseDto> Login(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }

            // Debug logging
            var debugInfo = $"🔍 DEBUG LOGIN for {user.Email}:\n" +
                           $"   PasswordHash: {user.PasswordHash?.Substring(0, Math.Min(20, user.PasswordHash?.Length ?? 0))}...\n" +
                           $"   Input password: {loginDto.Password}\n" +
                           $"   Is BCrypt hash: {user.PasswordHash?.StartsWith("$2")}";
            
            Console.WriteLine(debugInfo);
            System.Diagnostics.Debug.WriteLine(debugInfo);
            
            // Verify password with BCrypt
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            Console.WriteLine($"   BCrypt verification result: {isPasswordValid}");
            
            if (isPasswordValid)
            {
                var token = _jwtHelper.GenerateToken(user);
                return new LoginResponseDto
                {
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Success = true,
                    Message = "Login successful"
                };
            }

            Console.WriteLine($"   ❌ BCrypt verification failed!");
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid credentials"
            };
        }
    }
}
