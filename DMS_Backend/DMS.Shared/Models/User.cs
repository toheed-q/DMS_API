using System;
using System.ComponentModel.DataAnnotations;

namespace DMS.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Username { get; set; } = "";
        
        [Required]
        public string Password { get; set; } = "";  // In production, this should be hashed
        
        [Required]
        public string Role { get; set; } = "User";  // "Admin", "User" or "Salesman"

        /// <summary>
        /// Links a login account to a Salesman record. Required for mobile
        /// (Salesman-role) users — it's how the API scopes them to their own data.
        /// </summary>
        public int? SalesmanId { get; set; }
        
        public string? Email { get; set; } = "";
        
        public string? FullName { get; set; }
        
        public bool IsApproved { get; set; } = false;

        /// <summary>
        /// Master on/off switch for signing in. A new account is inactive by
        /// default (false) and cannot log in until an admin activates it —
        /// login is rejected with "You are not authorized" while this is false/null.
        /// </summary>
        public bool? IsAccountActive { get; set; } = false;

        public DateTime? CreatedAt { get; set; }
        
        public DateTime? ApprovedAt { get; set; }
        
        public int? ApprovedBy { get; set; }  // Admin who approved this user
        
        public byte[]? ProfilePicture { get; set; }  // Store profile picture as byte array
        
        // Permissions
        public bool CanAccessPOS { get; set; } = false;
        public bool CanAccessProducts { get; set; } = false;
        public bool CanAccessStockViewer { get; set; } = false;
        public bool CanAccessShops { get; set; } = false;
        public bool CanAccessRoutes { get; set; } = false;
        public bool CanAccessCategories { get; set; } = false;
        public bool CanAccessBillsHistory { get; set; } = false;
        public bool CanAccessEmployees { get; set; } = false;
        public bool CanAccessCompanies { get; set; } = false;

        // Security Questions
        public string? SecurityQuestion1 { get; set; }
        public string? SecurityAnswer1 { get; set; }
        public string? SecurityQuestion2 { get; set; }
        public string? SecurityAnswer2 { get; set; }
        public string? SecurityQuestion3 { get; set; }
        public string? SecurityAnswer3 { get; set; }
    }
}

