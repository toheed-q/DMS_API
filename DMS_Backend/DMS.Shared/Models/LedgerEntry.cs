using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DMS.Models
{
    public class LedgerEntry
    {
        [Key]
        public int LedgerEntryId { get; set; }

        public int ShopId { get; set; }
        public int? BillId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal BillTotal { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; }
        
        public DateTime EntryDate { get; set; } = DateTime.Now;

        public bool IsManualEntry { get; set; } = false;
        
        public bool IsReturn { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ReturnAmount { get; set; } = 0;

        // Salesman this return is attributed to (returns only). Optional so
        // existing/non-return entries are unaffected.
        public int? SalesmanId { get; set; }

        // Snapshot of the products returned (e.g. "Soap(2), Oil(1)"), captured
        // at save time so the salesman's return history reads correctly even if
        // a product is later renamed or removed.
        [MaxLength(1000)]
        public string? ReturnProducts { get; set; }

        [MaxLength(500)]
        public string? ManualItems { get; set; }

        // Navigation properties
        [ForeignKey("ShopId")]
        public virtual Shop Shop { get; set; } = null!;

        [ForeignKey("BillId")]
        public virtual Bill? Bill { get; set; }

        [ForeignKey("SalesmanId")]
        public virtual Salesman? Salesman { get; set; }

        [NotMapped]
        public string ItemNames => IsManualEntry ? (ManualItems ?? "") : 
            (Bill?.BillItems != null && Bill.BillItems.Any() 
            ? string.Join(", ", Bill.BillItems.Select(bi => $"{bi.ProductName}({bi.Quantity})")) 
            : "");
    }
}