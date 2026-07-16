namespace DMS_App.Services;

/// <summary>
/// Holds the signed-in session. The JWT goes to SecureStorage (Keystore / Keychain);
/// non-secret profile bits go to Preferences so the shell can greet the user without
/// decrypting anything.
/// </summary>
public interface ISessionStore
{
    Task SaveAsync(string token, DateTime? expiresAt, AuthUser user);
    Task<string?> GetTokenAsync();
    void Clear();

    int? SalesmanUserId { get; }
    string? Username { get; }
    string? FullName { get; }
    string? Role { get; }
}

public sealed class SessionStore : ISessionStore
{
    private const string TokenKey = "dms_auth_token";

    public async Task SaveAsync(string token, DateTime? expiresAt, AuthUser user)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);

        Preferences.Default.Set("dms_user_id", user.Id);
        Preferences.Default.Set("dms_username", user.Username);
        Preferences.Default.Set("dms_fullname", user.FullName);
        Preferences.Default.Set("dms_role", user.Role);
        if (expiresAt is { } exp)
            Preferences.Default.Set("dms_expires", exp.ToString("o"));
    }

    public Task<string?> GetTokenAsync() => SecureStorage.Default.GetAsync(TokenKey);

    public void Clear()
    {
        SecureStorage.Default.Remove(TokenKey);
        foreach (var key in new[] { "dms_user_id", "dms_username", "dms_fullname", "dms_role", "dms_expires" })
            Preferences.Default.Remove(key);
    }

    public int? SalesmanUserId =>
        Preferences.Default.ContainsKey("dms_user_id") ? Preferences.Default.Get("dms_user_id", 0) : null;

    public string? Username => Read("dms_username");
    public string? FullName => Read("dms_fullname");
    public string? Role => Read("dms_role");

    private static string? Read(string key)
    {
        var value = Preferences.Default.Get(key, string.Empty);
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
