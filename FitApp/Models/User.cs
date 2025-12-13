using System;
using System.ComponentModel.DataAnnotations;

namespace FitApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsEmailVerified { get; set; } = false;

        // Navigation property for one-to-one relation
        public UserDetails UserDetails { get; set; }
    }
}
