using DMS_Backend.Contracts;
using DMS_Backend.Data;
using DMS_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly JwtTokenService _jwt;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext db, JwtTokenService jwt, ILogger<AuthController> logger)
        {
            _db = db;
            _jwt = jwt;
            _logger = logger;
        }

        /// <summary>Authenticates a user and returns a JWT. Rate-limited to blunt brute-force attempts.</summary>
        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var username = request.Username.Trim();

            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            // Same generic message for "no user" and "wrong password" so we never
            // reveal which usernames exist.
            if (user is null || !PasswordHasher.Verify(request.Password, user.Password))
            {
                _logger.LogWarning("Failed login attempt for {Username}", username);
                return Unauthorized(new { message = "Invalid username or password." });
            }

            if (!user.IsApproved)
            {
                _logger.LogWarning("Login blocked for unapproved user {Username}", username);
                return Unauthorized(new { message = "Your account is pending approval." });
            }

            var (token, expiresAt) = _jwt.CreateToken(user);
            _logger.LogInformation("User {Username} (role {Role}) logged in", user.Username, user.Role);

            return Ok(new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.Role,
                    FullName = user.FullName,
                    Email = user.Email
                }
            });
        }
    }
}
