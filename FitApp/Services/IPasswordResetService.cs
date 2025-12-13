using FitApp.DTOs;

namespace FitApp.Services
{
    public interface IPasswordResetService
    {
        Task<PasswordForgotStartResponseDto> StartPasswordResetAsync(PasswordForgotStartDto request, string clientIp);
        Task<PasswordForgotVerifyResponseDto> VerifyPasswordResetCodeAsync(PasswordForgotVerifyDto request);
        Task<PasswordForgotResetResponseDto> ResetPasswordAsync(PasswordForgotResetDto request);
    }
}





