using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DMS_App.Services;

/// <summary>
/// Talks to POST /api/auth/login. Every failure mode — bad credentials, no network,
/// server down, timeout — is turned into a friendly <see cref="AuthResult"/> message
/// so the UI never has to catch exceptions.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthService(HttpClient http) => _http = http;

    public async Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest(username.Trim(), password),
                ct);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, ct);
                if (body is null || string.IsNullOrEmpty(body.Token) || body.User is null)
                    return AuthResult.Fail("The server sent an unexpected response. Please try again.");

                var u = body.User;
                return AuthResult.Ok(
                    body.Token,
                    body.ExpiresAt,
                    new AuthUser(u.Id, u.Username, u.Role, u.FullName ?? string.Empty, u.Email ?? string.Empty));
            }

            return AuthResult.Fail(await DescribeFailureAsync(response, ct));
        }
        catch (OperationCanceledException)
        {
            return AuthResult.Fail("The request timed out. Check your connection and try again.");
        }
        catch (HttpRequestException)
        {
            return AuthResult.Fail("Can't reach the server. Check your internet connection.");
        }
        catch (Exception)
        {
            return AuthResult.Fail("Something went wrong. Please try again.");
        }
    }

    private static async Task<string> DescribeFailureAsync(HttpResponseMessage response, CancellationToken ct)
    {
        // The API returns { "message": "..." } for handled failures.
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemBody>(JsonOptions, ct);
            if (!string.IsNullOrWhiteSpace(problem?.Message))
                return problem!.Message!;
        }
        catch
        {
            // fall through to a status-based message
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest => "Incorrect username or password.",
            HttpStatusCode.TooManyRequests => "Too many attempts. Please wait a minute and try again.",
            _ => "Sign in failed. Please try again."
        };
    }

    private sealed record LoginRequest(string Username, string Password);

    private sealed record LoginResponse(
        [property: JsonPropertyName("token")] string? Token,
        [property: JsonPropertyName("expiresAt")] DateTime? ExpiresAt,
        [property: JsonPropertyName("user")] UserPayload? User);

    private sealed record UserPayload(int Id, string Username, string Role, string? FullName, string? Email);

    private sealed record ProblemBody([property: JsonPropertyName("message")] string? Message);
}
