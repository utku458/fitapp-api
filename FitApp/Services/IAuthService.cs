using FitApp.DTOs;
using FitApp.Models;

namespace FitApp.Services
{
    public interface IAuthService
    {
        Task<User> Register(RegisterDto registerDto);
        Task<LoginResponseDto> Login(LoginDto loginDto);
    }
} 