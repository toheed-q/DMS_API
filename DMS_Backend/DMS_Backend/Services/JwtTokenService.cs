using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DMS.Models;
using DMS_Backend.Common;
using DMS_Backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DMS_Backend.Services
{
    /// <summary>Issues signed JWT access tokens for authenticated users.</summary>
    public class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _opt;

        public JwtTokenService(IOptions<JwtOptions> opt) => _opt = opt.Value;

        public (string token, DateTime expiresAt) CreateToken(User user)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_opt.ExpiryMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role),
            };
            if (!string.IsNullOrWhiteSpace(user.FullName))
                claims.Add(new Claim("fullName", user.FullName));

            // Mobile (Salesman) users carry the salesman record they map to — this is
            // what the API uses to scope them to their own data.
            if (user.SalesmanId.HasValue)
                claims.Add(new Claim(CurrentUserService.SalesmanIdClaim, user.SalesmanId.Value.ToString()));

            // Granular permissions, mirrored from the user's CanAccess* flags.
            AddPermission(claims, user.CanAccessPOS, Permissions.POS);
            AddPermission(claims, user.CanAccessProducts, Permissions.Products);
            AddPermission(claims, user.CanAccessStockViewer, Permissions.StockViewer);
            AddPermission(claims, user.CanAccessShops, Permissions.Shops);
            AddPermission(claims, user.CanAccessRoutes, Permissions.Routes);
            AddPermission(claims, user.CanAccessCategories, Permissions.Categories);
            AddPermission(claims, user.CanAccessBillsHistory, Permissions.BillsHistory);
            AddPermission(claims, user.CanAccessEmployees, Permissions.Employees);
            AddPermission(claims, user.CanAccessCompanies, Permissions.Companies);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        private static void AddPermission(List<Claim> claims, bool granted, string permission)
        {
            if (granted) claims.Add(new Claim(Permissions.ClaimType, permission));
        }
    }
}
