using System.ComponentModel.DataAnnotations;

namespace FitApp.Models
{
    public enum VerificationPurpose
    {
        Register,
        PasswordReset,
        EmailChange
    }

    public class EmailVerification
    {
        public Guid Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string CodeHash { get; set; } = string.Empty;
        
        [Required]
        public DateTime ExpiresAt { get; set; }
        
        public int Attempts { get; set; } = 0;
        
        public DateTime? LastSentAt { get; set; }
        
        public DateTime? ConsumedAt { get; set; }
        
        [Required]
        public VerificationPurpose Purpose { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

