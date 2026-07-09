using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    // Shared/API version: pure data model (no WPF binding/image-decoding logic).
    // The persisted columns match the desktop app exactly so the same database
    // schema is produced.
    public class Products
    {
        public int ProductsId { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public decimal CompanyPrice { get; set; }

        public decimal? RetailPrice { get; set; }
        public decimal? TradePrice { get; set; }
        public int? CategoryId { get; set; }
        public string? CompanyName { get; set; }   // Company/brand name for the product
        public string? Size { get; set; }           // e.g. "1-2 Small"

        public byte[]? ProductImage { get; set; }
        public string? ImageContentType { get; set; }
        public int? AvailableStock { get; set; }

        // Navigation property
        public Category? Category { get; set; }

        // Convenience (not stored)
        [NotMapped]
        public decimal TotalValue => (AvailableStock ?? 0) * (TradePrice ?? 0m);
    }
}
