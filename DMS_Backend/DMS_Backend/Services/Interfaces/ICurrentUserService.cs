namespace DMS_Backend.Services.Interfaces
{
    /// <summary>
    /// The user behind the current request, read from the JWT. Lets services record
    /// "who did this" (audit trail) without depending on HTTP types themselves.
    /// </summary>
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? Username { get; }
        string? Role { get; }
        bool IsAdmin { get; }
    }
}
