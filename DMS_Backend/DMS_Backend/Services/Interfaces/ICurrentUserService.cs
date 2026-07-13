namespace DMS_Backend.Services.Interfaces
{
    /// <summary>
    /// The user behind the current request, read from the JWT. Lets services record
    /// "who did this" (audit trail) and enforce data scoping — without depending on
    /// HTTP types themselves.
    /// </summary>
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? Username { get; }
        string? Role { get; }

        /// <summary>Set for Salesman-role (mobile) users — the Salesman record they are.</summary>
        int? SalesmanId { get; }

        bool IsAdmin { get; }

        /// <summary>Field salesman: may only see and create their OWN data.</summary>
        bool IsSalesman { get; }

        /// <summary>Granular permission check (admins always pass).</summary>
        bool HasPermission(string permission);
    }
}
