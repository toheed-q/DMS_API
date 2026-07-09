using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    /// <summary>
    /// A payment received from a customer (shop) recorded at the LEDGER level,
    /// not against any individual bill. Outstanding/excess are computed by
    /// aggregating these against the customer's total bills.
    /// </summary>
    public class LedgerReceiving
    {
        [Key]
        public int LedgerReceivingId { get; set; }

        public int ShopId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime ReceivedDate { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? Remarks { get; set; }

        // Who recorded it (captured from the logged-in user at save time).
        public int? CreatedByUserId { get; set; }

        [MaxLength(100)]
        public string? CreatedByName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ShopId")]
        public virtual Shop Shop { get; set; } = null!;
    }
}
