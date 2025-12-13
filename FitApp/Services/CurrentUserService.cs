using System.Security.Claims;

namespace FitApp.Services
{
    /// <summary>
    /// Mevcut kullanıcı bilgilerini sağlayan servis implementasyonu
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
                
                if (int.TryParse(userIdClaim, out var userId))
                    return userId;
                
                return null;
            }
        }

        public string? Email
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
        }
    }
}
