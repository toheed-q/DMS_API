namespace DMS_App.Services;

/// <summary>The signed-in user, as returned by the API's login endpoint.</summary>
public sealed record AuthUser(int Id, string Username, string Role, string FullName, string Email);

/// <summary>Outcome of a login attempt. On failure, <see cref="Error"/> is user-facing.</summary>
public sealed record AuthResult(
    bool Success,
    string? Token = null,
    DateTime? ExpiresAt = null,
    AuthUser? User = null,
    string? Error = null)
{
    public static AuthResult Ok(string token, DateTime? expiresAt, AuthUser user) =>
        new(true, token, expiresAt, user);

    public static AuthResult Fail(string error) => new(false, Error: error);
}

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default);
}
