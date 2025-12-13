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
    public interface IEmailVerificationService
    {
        Task<RegisterStartResponseDto> StartRegistrationAsync(RegisterStartDto request, string clientIp);
        Task<RegisterVerifyResponseDto> VerifyRegistrationAsync(RegisterVerifyDto request);
        Task<RegisterResendResponseDto> ResendVerificationAsync(RegisterResendDto request, string clientIp);
    }

    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EmailVerificationService> _logger;
        private readonly JwtHelper _jwtHelper;

        public EmailVerificationService(
            ApplicationDbContext context,
            IEmailSender emailSender,
            IMemoryCache cache,
            ILogger<EmailVerificationService> logger,
            JwtHelper jwtHelper)
        {
            _context = context;
            _emailSender = emailSender;
            _cache = cache;
            _logger = logger;
            _jwtHelper = jwtHelper;
        }

        public async Task<RegisterStartResponseDto> StartRegistrationAsync(RegisterStartDto request, string clientIp)
        {
            try
            {
                // Rate limiting check
                if (IsRateLimited(clientIp, "register_start", 5, TimeSpan.FromMinutes(1)))
                {
                    return new RegisterStartResponseDto
                    {
                        Message = "Too many registration attempts. Please try again later.",
                        ResendAfterSeconds = 60
                    };
                }

                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                {
                    return new RegisterStartResponseDto
                    {
                        Message = "User with this email already exists.",
                        ResendAfterSeconds = 0
                    };
                }

                // Create user with unverified email
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);
                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Name = request.Email.Split('@')[0], // Default name from email
                    IsEmailVerified = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate verification code
                var code = GenerateVerificationCode();
                var codeHash = HashCode(code);

                // Create email verification record
                var verification = new EmailVerification
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    CodeHash = codeHash,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    Purpose = VerificationPurpose.Register,
                    LastSentAt = DateTime.UtcNow
                };

                _context.EmailVerifications.Add(verification);
                await _context.SaveChangesAsync();

                // Send email
                var htmlBody = $"FitAppV2 Doğrulama Kodun: <b>{code}</b> – 10 dk geçerli";
                var emailSent = await _emailSender.SendAsync(request.Email, "FitAppV2 E-posta Doğrulama", htmlBody);

                if (!emailSent)
                {
                    _logger.LogError("Failed to send verification email to {Email}", request.Email);
                    return new RegisterStartResponseDto
                    {
                        Message = "Failed to send verification email. Please try again.",
                        ResendAfterSeconds = 60
                    };
                }

                var maskedEmail = MaskEmail(request.Email);

                return new RegisterStartResponseDto
                {
                    VerificationId = verification.Id,
                    MaskedEmail = maskedEmail,
                    ResendAfterSeconds = 60,
                    Message = "Verification email sent successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StartRegistrationAsync for email {Email}", request.Email);
                throw;
            }
        }

        public async Task<RegisterVerifyResponseDto> VerifyRegistrationAsync(RegisterVerifyDto request)
        {
            try
            {
                var verification = await _context.EmailVerifications
                    .FirstOrDefaultAsync(ev => ev.Id == request.VerificationId && 
                                              ev.Purpose == VerificationPurpose.Register);

                if (verification == null)
                {
                    return new RegisterVerifyResponseDto
                    {
                        Message = "Invalid verification ID."
                    };
                }

                // Check if expired
                if (verification.ExpiresAt < DateTime.UtcNow)
                {
                    return new RegisterVerifyResponseDto
                    {
                        Message = "Verification code has expired. Please request a new one."
                    };
                }

                // Check attempts
                if (verification.Attempts >= 5)
                {
                    return new RegisterVerifyResponseDto
                    {
                        Message = "Too many failed attempts. Please request a new verification code."
                    };
                }

                // Verify code
                var codeHash = HashCode(request.Code);
                if (verification.CodeHash != codeHash)
                {
                    verification.Attempts++;
                    await _context.SaveChangesAsync();

                    return new RegisterVerifyResponseDto
                    {
                        Message = $"Invalid verification code. {5 - verification.Attempts} attempts remaining."
                    };
                }

                // Check if already consumed
                if (verification.ConsumedAt.HasValue)
                {
                    return new RegisterVerifyResponseDto
                    {
                        Message = "Verification code has already been used."
                    };
                }

                // Mark as consumed
                verification.ConsumedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Update user as verified
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == verification.Email);

                if (user == null)
                {
                    return new RegisterVerifyResponseDto
                    {
                        Message = "User not found."
                    };
                }

                user.IsEmailVerified = true;
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = _jwtHelper.GenerateToken(user);

                return new RegisterVerifyResponseDto
                {
                    Token = token,
                    Email = user.Email,
                    Name = user.Name,
                    Message = "Email verified successfully!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyRegistrationAsync for verification ID {VerificationId}", request.VerificationId);
                throw;
            }
        }

        public async Task<RegisterResendResponseDto> ResendVerificationAsync(RegisterResendDto request, string clientIp)
        {
            try
            {
                // Rate limiting check
                if (IsRateLimited(clientIp, "register_resend", 5, TimeSpan.FromMinutes(1)))
                {
                    return new RegisterResendResponseDto
                    {
                        Message = "Too many resend attempts. Please try again later.",
                        ResendAfterSeconds = 60
                    };
                }

                var verification = await _context.EmailVerifications
                    .FirstOrDefaultAsync(ev => ev.Id == request.VerificationId && 
                                              ev.Purpose == VerificationPurpose.Register);

                if (verification == null)
                {
                    return new RegisterResendResponseDto
                    {
                        Message = "Invalid verification ID."
                    };
                }

                // Check if already consumed
                if (verification.ConsumedAt.HasValue)
                {
                    return new RegisterResendResponseDto
                    {
                        Message = "Verification code has already been used."
                    };
                }

                // Check resend cooldown (60 seconds)
                if (verification.LastSentAt.HasValue && 
                    DateTime.UtcNow < verification.LastSentAt.Value.AddSeconds(60))
                {
                    var remainingSeconds = (int)(verification.LastSentAt.Value.AddSeconds(60) - DateTime.UtcNow).TotalSeconds;
                    return new RegisterResendResponseDto
                    {
                        Message = $"Please wait {remainingSeconds} seconds before requesting a new code.",
                        ResendAfterSeconds = remainingSeconds
                    };
                }

                // Generate new code
                var code = GenerateVerificationCode();
                var codeHash = HashCode(code);

                // Update verification record
                verification.CodeHash = codeHash;
                verification.ExpiresAt = DateTime.UtcNow.AddMinutes(10);
                verification.Attempts = 0;
                verification.LastSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Send email
                var htmlBody = $"FitAppV2 Doğrulama Kodun: <b>{code}</b> – 10 dk geçerli";
                var emailSent = await _emailSender.SendAsync(verification.Email, "FitAppV2 E-posta Doğrulama", htmlBody);

                if (!emailSent)
                {
                    _logger.LogError("Failed to send verification email to {Email}", verification.Email);
                    return new RegisterResendResponseDto
                    {
                        Message = "Failed to send verification email. Please try again.",
                        ResendAfterSeconds = 60
                    };
                }

                return new RegisterResendResponseDto
                {
                    Message = "Verification email sent successfully.",
                    ResendAfterSeconds = 60
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResendVerificationAsync for verification ID {VerificationId}", request.VerificationId);
                throw;
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

        private string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2) return email;

            var username = parts[0];
            var domain = parts[1];

            if (username.Length <= 2)
                return $"{username}***@{domain}";

            var maskedUsername = username.Substring(0, 2) + "***";
            return $"{maskedUsername}@{domain}";
        }

        private bool IsRateLimited(string key, string operation, int maxAttempts, TimeSpan window)
        {
            var cacheKey = $"rate_limit:{operation}:{key}";
            
            if (_cache.TryGetValue(cacheKey, out int attempts))
            {
                if (attempts >= maxAttempts)
                    return true;
                
                _cache.Set(cacheKey, attempts + 1, window);
            }
            else
            {
                _cache.Set(cacheKey, 1, window);
            }
            
            return false;
        }
    }
}
