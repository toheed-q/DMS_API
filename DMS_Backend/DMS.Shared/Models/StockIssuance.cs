using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class StockIssuance
    {
        [Key]
        public int StockIssuanceId { get; set; }

        [Required]
        public int SalesmanId { get; set; }

        [ForeignKey("SalesmanId")]
        public virtual Salesman Salesman { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Products Product { get; set; } = null!;

        [Required]
        public int IssuedQuantity { get; set; } = 0;

        [Required]
        public DateTime IssuedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
