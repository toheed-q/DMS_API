using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class DSRExpense
    {
        [Key]
        public int DSRExpenseId { get; set; }

        [Required]
        public int DSRId { get; set; }

        [ForeignKey("DSRId")]
        public virtual DailySalesReport DailySalesReport { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string ExpenseName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0;
    }
}
