using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Data;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Services
{
    /// <summary>
    /// Authentication business logic. Controllers call this and never touch the
    /// DbContext directly (SRP: HTTP concerns stay in the controller, rules here).
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext db,
            ITokenService tokenService,
            IPasswordHasher passwordHasher,
            ILogger<AuthService> logger)
        {
            _db = db;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var username = request.Username.Trim();

            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            // Same generic message for "no user" and "wrong password" so we never
            // reveal which usernames exist.
            if (user is null || !_passwordHasher.Verify(request.Password, user.Password))
            {
                _logger.LogWarning("Failed login attempt for {Username}", username);
                return Result<LoginResponse>.Failure("Invalid username or password.", ErrorType.Unauthorized);
            }

            if (!user.IsApproved)
            {
                _logger.LogWarning("Login blocked for unapproved user {Username}", username);
                return Result<LoginResponse>.Failure("Your account is pending approval.", ErrorType.Unauthorized);
            }

            var (token, expiresAt) = _tokenService.CreateToken(user);
            _logger.LogInformation("User {Username} (role {Role}) logged in", user.Username, user.Role);

            return Result<LoginResponse>.Success(new LoginResponse
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
