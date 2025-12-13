using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using FitApp.Data;
using FitApp.Models;
using FitApp.Services;
using FitApp.DTOs;
using FitApp.Helpers;

namespace FitApp.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PasswordResetService> _logger;
        private readonly JwtHelper _jwtHelper;

        public PasswordResetService(
            ApplicationDbContext context,
            IEmailSender emailSender,
            IMemoryCache cache,
            ILogger<PasswordResetService> logger,
            JwtHelper jwtHelper)
        {
            _context = context;
            _emailSender = emailSender;
            _cache = cache;
            _logger = logger;
            _jwtHelper = jwtHelper;
        }

        public async Task<PasswordForgotStartResponseDto> StartPasswordResetAsync(PasswordForgotStartDto request, string clientIp)
        {
            try
            {
                var response = new PasswordForgotStartResponseDto
                {
                    Message = "If an account exists, a code has been sent.",
                    ResendAfterSeconds = 60
                };

                // Check rate limiting
                var cacheKey = $"password_reset_rate_limit_{clientIp}";
                if (_cache.TryGetValue(cacheKey, out int attemptCount) && attemptCount >= 5)
                {
                    response.ResendAfterSeconds = 300; // 5 minutes
                    return response;
                }

                // Check if user exists (but don't reveal this information)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    // Still return success message for security
                    return response;
                }

                // Check for recent verification to enforce cooldown
                var recentVerification = await _context.EmailVerifications
                    .Where(ev => ev.Email == request.Email &&
                                ev.Purpose == VerificationPurpose.PasswordReset &&
                                ev.LastSentAt.HasValue &&
                                ev.LastSentAt.Value > DateTime.UtcNow.AddSeconds(-60))
                    .OrderByDescending(ev => ev.LastSentAt)
                    .FirstOrDefaultAsync();

                if (recentVerification != null)
                {
                    // Check resend cooldown (60 seconds)
                    var remainingSeconds = (int)(recentVerification.LastSentAt.Value.AddSeconds(60) - DateTime.UtcNow).TotalSeconds;
                    response.ResendAfterSeconds = remainingSeconds;
                    return response;
                }

                // Mark all existing verifications for this email as consumed (invalidate old codes)
                var existingVerifications = await _context.EmailVerifications
                    .Where(ev => ev.Email == request.Email && 
                                ev.Purpose == VerificationPurpose.PasswordReset &&
                                ev.ConsumedAt == null)
                    .ToListAsync();

                foreach (var existing in existingVerifications)
                {
                    existing.ConsumedAt = DateTime.UtcNow;
                }

                // Create new verification record
                var code = GenerateVerificationCode();
                var codeHash = HashCode(code);

                var verification = new EmailVerification
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    CodeHash = codeHash,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    Purpose = VerificationPurpose.PasswordReset,
                    LastSentAt = DateTime.UtcNow
                };

                _context.EmailVerifications.Add(verification);
                var codeToSend = code;
                _logger.LogInformation("Password reset code sent for new verification: {Code}", code);

                await _context.SaveChangesAsync();

                // Send email (only if user exists)
                // Use the same code that was saved to database
                
                var htmlBody = $"FitAppV2 Şifre Sıfırlama Kodun: <b>{codeToSend}</b> – 10 dk geçerli";
                var emailSent = await _emailSender.SendAsync(request.Email, "FitAppV2 Şifre Sıfırlama", htmlBody);

                if (!emailSent)
                {
                    _logger.LogError("Failed to send password reset email to {Email}", request.Email);
                    // Still return success message for security
                }

                // Update rate limiting
                _cache.Set(cacheKey, (attemptCount + 1), TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StartPasswordResetAsync for email {Email}", request.Email);
                // Return generic message for security
                return new PasswordForgotStartResponseDto
                {
                    Message = "If an account exists, a code has been sent.",
                    ResendAfterSeconds = 60
                };
            }
        }

        public async Task<PasswordForgotVerifyResponseDto> VerifyPasswordResetCodeAsync(PasswordForgotVerifyDto request)
        {
            try
            {
                var verification = await _context.EmailVerifications
                    .Where(ev => ev.Email == request.Email && 
                                ev.Purpose == VerificationPurpose.PasswordReset &&
                                ev.ConsumedAt == null)
                    .OrderByDescending(ev => ev.LastSentAt)
                    .FirstOrDefaultAsync();

                if (verification == null)
                {
                    return new PasswordForgotVerifyResponseDto
                    {
                        Message = "Invalid verification code or email.",
                        IsSuccess = false,
                        ResetToken = null,
                        ExpiresInSeconds = null
                    };
                }

                // Check if expired
                if (verification.ExpiresAt < DateTime.UtcNow)
                {
                    return new PasswordForgotVerifyResponseDto
                    {
                        Message = "Verification code has expired. Please request a new one.",
                        IsSuccess = false,
                        ResetToken = null,
                        ExpiresInSeconds = null
                    };
                }

                // Check attempts
                if (verification.Attempts >= 5)
                {
                    return new PasswordForgotVerifyResponseDto
                    {
                        Message = "Too many failed attempts. Please request a new verification code.",
                        IsSuccess = false,
                        ResetToken = null,
                        ExpiresInSeconds = null
                    };
                }

                // Verify code
                var codeHash = HashCode(request.Code);
                if (verification.CodeHash != codeHash)
                {
                    verification.Attempts++;
                    await _context.SaveChangesAsync();

                    return new PasswordForgotVerifyResponseDto
                    {
                        Message = $"Invalid verification code. {5 - verification.Attempts} attempts remaining.",
                        IsSuccess = false,
                        ResetToken = null,
                        ExpiresInSeconds = null
                    };
                }

                // Check if already consumed
                if (verification.ConsumedAt.HasValue)
                {
                    return new PasswordForgotVerifyResponseDto
                    {
                        Message = "Verification code has already been used.",
                        IsSuccess = false,
                        ResetToken = null,
                        ExpiresInSeconds = null
                    };
                }

                // Mark as consumed
                verification.ConsumedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate reset token (JWT with short expiry and specific scope)
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return new PasswordForgotVerifyResponseDto
                    {
                        Message = "User not found.",
                        IsSuccess = false,
                        ResetToken = null,
                        ExpiresInSeconds = null
                    };
                }

                var resetToken = _jwtHelper.GeneratePasswordResetToken(user);

                return new PasswordForgotVerifyResponseDto
                {
                    ResetToken = resetToken,
                    ExpiresInSeconds = 300, // 5 minutes (300 seconds)
                    Message = "Verification successful. You can now reset your password.",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyPasswordResetCodeAsync for email {Email}", request.Email);
                return new PasswordForgotVerifyResponseDto
                {
                    Message = "An error occurred during verification. Please try again.",
                    IsSuccess = false,
                    ResetToken = null,
                    ExpiresInSeconds = null
                };
            }
        }

        public async Task<PasswordForgotResetResponseDto> ResetPasswordAsync(PasswordForgotResetDto request)
        {
            try
            {
                // Validate reset token
                var user = _jwtHelper.ValidatePasswordResetToken(request.ResetToken);
                if (user == null)
                {
                    return new PasswordForgotResetResponseDto
                    {
                        Message = "Invalid or expired reset token. Please request a new one.",
                        IsSuccess = false
                    };
                }

                // Validate password strength
                if (!IsPasswordStrong(request.NewPassword))
                {
                    return new PasswordForgotResetResponseDto
                    {
                        Message = "Password must be at least 8 characters long and contain uppercase, lowercase, number, and special character.",
                        IsSuccess = false
                    };
                }

                // Get user from database
                var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (dbUser == null)
                {
                    return new PasswordForgotResetResponseDto
                    {
                        Message = "User not found.",
                        IsSuccess = false
                    };
                }

                // Hash new password using BCrypt
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
                dbUser.PasswordHash = passwordHash;

                // Invalidate all existing refresh tokens (security measure)
                // Note: This would require a RefreshToken table in your database
                // For now, we'll just update the password

                await _context.SaveChangesAsync();

                _logger.LogInformation("Password reset successful for user {Email}", user.Email);

                return new PasswordForgotResetResponseDto
                {
                    Message = "Password has been reset successfully. Please log in with your new password.",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPasswordAsync");
                return new PasswordForgotResetResponseDto
                {
                    Message = "An error occurred while resetting the password. Please try again.",
                    IsSuccess = false
                };
            }
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string HashCode(string code)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(hashBytes);
        }



        private bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            var hasUpper = password.Any(char.IsUpper);
            var hasLower = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
    }
}
