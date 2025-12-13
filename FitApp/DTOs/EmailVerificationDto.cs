using System.ComponentModel.DataAnnotations;

namespace FitApp.DTOs
{
    public class RegisterStartDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterVerifyDto
    {
        [Required]
        public Guid VerificationId { get; set; }
        
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;
    }

    public class RegisterResendDto
    {
        [Required]
        public Guid VerificationId { get; set; }
    }

    public class RegisterStartResponseDto
    {
        public Guid VerificationId { get; set; }
        public string MaskedEmail { get; set; } = string.Empty;
        public int ResendAfterSeconds { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class RegisterVerifyResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class RegisterResendResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int ResendAfterSeconds { get; set; }
    }
}

