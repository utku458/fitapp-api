using System.ComponentModel.DataAnnotations;

namespace FitApp.DTOs
{
    public class PasswordForgotStartDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class PasswordForgotStartResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int ResendAfterSeconds { get; set; }
    }

    public class PasswordForgotVerifyDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;
    }

    public class PasswordForgotVerifyResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string? ResetToken { get; set; }
        public int? ExpiresInSeconds { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class PasswordForgotResetDto
    {
        [Required]
        public string ResetToken { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain at least 8 characters, one uppercase letter, one lowercase letter, one number and one special character")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class PasswordForgotResetResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}
