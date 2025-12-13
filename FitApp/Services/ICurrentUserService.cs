namespace FitApp.Services
{
    /// <summary>
    /// Mevcut kullanıcı bilgilerini sağlayan servis
    /// Global Query Filter için kullanılır
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Mevcut kullanıcının ID'si
        /// </summary>
        int? UserId { get; }
        
        /// <summary>
        /// Mevcut kullanıcının email'i
        /// </summary>
        string? Email { get; }
        
        /// <summary>
        /// Kullanıcı kimlik doğrulaması yapılmış mı?
        /// </summary>
        bool IsAuthenticated { get; }
    }
}
