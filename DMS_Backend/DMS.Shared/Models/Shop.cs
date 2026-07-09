using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class Shop
    {
        [Key]
        public int ShopId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Address { get; set; }
        
        public string? ContactNumber { get; set; }

        public string? NTNNumber { get; set; }
        public string? FBRNumber { get; set; }
        public string? OtherTaxNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key to Route
        public int? RouteId { get; set; }

        [ForeignKey("RouteId")]
        public virtual Route? Route { get; set; }

        // Navigation property for Bills
        public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
        
        // Navigation property for Ledger Entries
        public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    }
}