using System.ComponentModel.DataAnnotations;

namespace DMS_Backend.Contracts
{
    public class CreateUserRequest
    {
        [Required, MinLength(3)]
        public string Username { get; set; } = string.Empty;

        [Required, MinLength(4, ErrorMessage = "Password must be at least 4 characters.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>"Admin", "User" or "Salesman".</summary>
        [Required]
        public string Role { get; set; } = "User";

        /// <summary>
        /// For a "Salesman" login, links it to an EXISTING salesman record. Leave this
        /// null to have a new salesman record created automatically from the fields
        /// below — that way one call sets up both the employee record and the login.
        /// </summary>
        public int? SalesmanId { get; set; }

        public string? FullName { get; set; }
        public string? Email { get; set; }

        // --- Used only when creating a new salesman record (Role = "Salesman" and
        //     SalesmanId is null). Ignored otherwise. ---

        /// <summary>Required when a new salesman record is being created.</summary>
        public string? PhoneNumber { get; set; }

        /// <summary>Format: 11111-1111111-1</summary>
        public string? CNIC { get; set; }

        public decimal MonthlySalary { get; set; }

        /// <summary>
        /// Whether the account may sign in. Defaults to false — an inactive account
        /// is created but cannot log in until explicitly activated.
        /// </summary>
        public bool IsAccountActive { get; set; } = false;

        // Granular permissions (ignored for Admin, who has everything).
        public bool CanAccessPOS { get; set; }
        public bool CanAccessProducts { get; set; }
        public bool CanAccessStockViewer { get; set; }
        public bool CanAccessShops { get; set; }
        public bool CanAccessRoutes { get; set; }
        public bool CanAccessCategories { get; set; }
        public bool CanAccessBillsHistory { get; set; }
        public bool CanAccessEmployees { get; set; }
        public bool CanAccessCompanies { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? SalesmanId { get; set; }
        public string? SalesmanName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public bool IsApproved { get; set; }
        public bool IsAccountActive { get; set; }
        public List<string> Permissions { get; set; } = [];
    }
}
