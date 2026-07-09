using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class Bill
    {
        [Key]
        public int BillId { get; set; }

        public int? ShopId { get; set; }

        [ForeignKey("ShopId")]
        public virtual Shop? Shop { get; set; }

        [Required]
        public DateTime BillDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GSTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdvanceTaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalesTaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherTaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousDues { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        public bool ApplyGST { get; set; }
        public bool ApplyAdvanceTax { get; set; }
        public bool ApplySalesTax { get; set; }
        public bool ApplyOtherTax { get; set; }

        [MaxLength(200)]
        public string? WalkInCustomerName { get; set; }

        [MaxLength(50)]
        public string? WalkInCustomerPhone { get; set; }

        // Optional: Link to Salesman (Employee)
        public int? SMId { get; set; }

        [ForeignKey("SMId")]
        public virtual Salesman? Salesman { get; set; }

        // Navigation property for Bill Items
        public virtual ICollection<BillItem> BillItems { get; set; } = new List<BillItem>();

        // Optional: Link to User who created the bill
        public int? CreatedByUserId { get; set; }

        // In-memory only: set by views (e.g. Bills History) after reconciling
        // against the ledger / payment receivings. Not persisted.
        [NotMapped]
        public bool IsPaid { get; set; }
    }
}
