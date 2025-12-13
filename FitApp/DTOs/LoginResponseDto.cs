namespace FitApp.DTOs
{
    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
} 