using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models;

public class CompanyPaymentDetail
{
    [Key]
    public int PaymentDetailId { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [Required]
    [MaxLength(200)]
    public string BankName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AccountNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation property
    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;
}
