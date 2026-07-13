using System.Security.Claims;
using DMS_Backend.Common;
using DMS_Backend.Services.Interfaces;

namespace DMS_Backend.Services
{
    /// <summary>Reads the authenticated user's identity from the request's JWT claims.</summary>
    public class CurrentUserService : ICurrentUserService
    {
        public const string SalesmanIdClaim = "salesmanId";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public int? UserId =>
            int.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        public string? Username => User?.FindFirstValue(ClaimTypes.Name);

        public string? Role => User?.FindFirstValue(ClaimTypes.Role);

        public int? SalesmanId =>
            int.TryParse(User?.FindFirstValue(SalesmanIdClaim), out var id) ? id : null;

        public bool IsAdmin => string.Equals(Role, Roles.Admin, StringComparison.OrdinalIgnoreCase);

        public bool IsSalesman => string.Equals(Role, Roles.Salesman, StringComparison.OrdinalIgnoreCase);

        public bool HasPermission(string permission)
            => IsAdmin || (User?.HasClaim(Permissions.ClaimType, permission) ?? false);
    }
}
