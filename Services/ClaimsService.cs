using System.Security.Claims;
using HR_Management_System.Interfaces;

namespace HR_Management_System.Services
{
    public class ClaimsService : IClaimsService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // =====================
        // Get Logged-in User ID
        // =====================
        public Guid GetUserId()
        {
            var value = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(value))
                throw new UnauthorizedAccessException("User ID not found in token");

            return Guid.Parse(value);
        }

        public Guid GetOrganizationId()
        {
            var value = _httpContextAccessor.HttpContext?.User
                .FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(value))
                throw new UnauthorizedAccessException("Organization ID not found in token");

            return Guid.Parse(value);
        }

        // =====================
        // Get Role
        // =====================
        public string GetRole()
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}