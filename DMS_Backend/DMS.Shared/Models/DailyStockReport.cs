using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class DailyStockReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ShopName { get; set; } = "";

        public string? Address { get; set; }

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBill { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidBill { get; set; }

        [NotMapped]
        public decimal DueBill => TotalBill - PaidBill;

        [Required]
        public DateTime ReportDate { get; set; } = DateTime.Today;

        public string? ContactNumber { get; set; }

        public string? PaymentMode { get; set; }

        // Approval fields
        public string ApprovalStatus { get; set; } = "Approved"; // "Approved", "Pending", "Rejected"
        
        public string? Remarks { get; set; }
        
        public DateTime? SubmittedAt { get; set; }
        
        public DateTime? ApprovedAt { get; set; }
        
        public int? SubmittedBy { get; set; } // User ID who submitted
        
        public int? ApprovedBy { get; set; } // Admin ID who approved/rejected

        // Delete approval fields
        public bool IsDeleteRequested { get; set; } = false;
        
        public DateTime? DeleteRequestedAt { get; set; }
        
        public int? DeleteRequestedBy { get; set; } // User ID who requested deletion

        [NotMapped]
        public bool IsSelected { get; set; }
    }
}
