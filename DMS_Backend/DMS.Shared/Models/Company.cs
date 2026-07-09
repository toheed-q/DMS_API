using System.ComponentModel.DataAnnotations;

namespace DMS.Models;

public class Company
{
    [Key]
    public int CompanyId { get; set; }

    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? ContactNumber { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<CompanyPaymentDetail> PaymentDetails { get; set; } = new List<CompanyPaymentDetail>();
    public ICollection<CompanyLedgerEntry> LedgerEntries { get; set; } = new List<CompanyLedgerEntry>();
}
