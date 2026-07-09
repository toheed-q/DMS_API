using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class DSRItem
    {
        [Key]
        public int DSRItemId { get; set; }

        [Required]
        public int DSRId { get; set; }

        [ForeignKey("DSRId")]
        public virtual DailySalesReport DailySalesReport { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Products Product { get; set; } = null!;

        [Required]
        public int StockLoaded { get; set; } = 0;

        [Required]
        public int ReturnQty { get; set; } = 0;

        [NotMapped]
        public int SaleQty => StockLoaded - ReturnQty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TradePrice { get; set; } = 0;

        public bool IsDiscountPercentage { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; } = 0;

        [NotMapped]
        public decimal TotalAmount
        {
            get
            {
                var baseAmount = SaleQty * TradePrice;
                var discount = IsDiscountPercentage 
                    ? (baseAmount * DiscountValue / 100) 
                    : DiscountValue;
                return baseAmount - discount;
            }
        }
    }
}
