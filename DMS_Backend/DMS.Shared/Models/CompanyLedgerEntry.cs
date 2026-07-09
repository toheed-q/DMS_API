using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models;

public class CompanyLedgerEntry
{
    [Key]
    public int LedgerEntryId { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [MaxLength(100)]
    public string? InvoiceNumber { get; set; }

    public double InvoiceAmount { get; set; }

    public double Fare { get; set; }

    public double Claim { get; set; }

    // Auto-calculated: InvoiceAmount - (Fare + Claim)
    public double NetPayment { get; set; }

    [MaxLength(50)]
    public string? ModeOfPayment { get; set; }

    [MaxLength(100)]
    public string? AccountNumber { get; set; }

    [MaxLength(100)]
    public string? TransactionId { get; set; }

    public double PaidAmount { get; set; }

    // Auto-calculated: NetPayment - PaidAmount (never negative)
    public double RemainingAmount { get; set; }

    // Excess/Advance payment amount
    public double ExcessAmount { get; set; }

    public byte[]? Attachment { get; set; }

    public DateTime EntryDate { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;
}
