using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class Salesman
    {
        [Key]
        public int SalesmanId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [RegularExpression(@"^\d{5}-\d{7}-\d{1}$", ErrorMessage = "CNIC must be in format: 11111-1111111-1")]
        public string? CNIC { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlySalary { get; set; } = 0;

        public string? IncentiveType { get; set; } // "Percentage" or "Fixed"

        [Column(TypeName = "decimal(18,2)")]
        public decimal? IncentiveValue { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<DailySalesReport> DailySalesReports { get; set; } = new List<DailySalesReport>();
    }
}
