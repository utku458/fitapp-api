using Microsoft.AspNetCore.Mvc;
using FitApp.DTOs;
using FitApp.Models;
using FitApp.Services;

namespace FitApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IPasswordResetService _passwordResetService;

        public AuthController(IAuthService authService, IPasswordResetService passwordResetService)
        {
            _authService = authService;
            _passwordResetService = passwordResetService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                var user = await _authService.Register(registerDto);
                return Ok(new 
                { 
                    success = true,
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var response = await _authService.Login(loginDto);
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(new { message = response.Message });
            }
        }

        [HttpPost("password/forgot/start")]
        public async Task<IActionResult> StartPasswordReset(PasswordForgotStartDto request)
        {
            try
            {
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var response = await _passwordResetService.StartPasswordResetAsync(request, clientIp);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("password/forgot/verify")]
        public async Task<IActionResult> VerifyPasswordResetCode(PasswordForgotVerifyDto request)
        {
            try
            {
                var response = await _passwordResetService.VerifyPasswordResetCodeAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("password/forgot/reset")]
        public async Task<IActionResult> ResetPassword(PasswordForgotResetDto request)
        {
            try
            {
                var response = await _passwordResetService.ResetPasswordAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}