using DMS_Backend.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DMS_Backend.Authorization
{
    /// <summary>
    /// Requires a granular permission (mirrors the User's CanAccess* flags).
    /// Admins always pass. Unauthenticated -> 401, missing permission -> 403.
    /// Usage: [RequirePermission(Permissions.Products)]
    /// </summary>
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string permission)
            : base(typeof(PermissionFilter))
            => Arguments = [permission];
    }

    public class PermissionFilter : IAuthorizationFilter
    {
        private readonly string _permission;

        public PermissionFilter(string permission) => _permission = permission;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (user.IsInRole(Roles.Admin))
                return;

            if (!user.HasClaim(Permissions.ClaimType, _permission))
                context.Result = new ForbidResult();
        }
    }
}
