using System.ComponentModel.DataAnnotations;

namespace DMS_Backend.Contracts
{
    public class CreateBillItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        /// <summary>Optional. Defaults to the product's trade price.</summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>Optional per-unit override price. When set (&gt; 0), the line is
        /// charged at this price instead of UnitPrice.</summary>
        public decimal? DiscountRate { get; set; }
    }

    public class CreateBillRequest
    {
        /// <summary>Shop (customer) sale. Leave null for a walk-in sale.</summary>
        public int? ShopId { get; set; }

        public string? WalkInCustomerName { get; set; }
        public string? WalkInCustomerPhone { get; set; }

        /// <summary>Salesman the sale is attributed to.</summary>
        public int? SalesmanId { get; set; }

        /// <summary>Defaults to now.</summary>
        public DateTime? BillDate { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "A bill must contain at least one item.")]
        public List<CreateBillItemRequest> Items { get; set; } = [];

        /// <summary>Bill-level discount: a percentage if IsPercentageDiscount, else an amount.</summary>
        public decimal Discount { get; set; }
        public bool IsPercentageDiscount { get; set; }

        public bool ApplyGST { get; set; }
        public bool ApplyAdvanceTax { get; set; }
        public bool ApplySalesTax { get; set; }
        public bool ApplyOtherTax { get; set; }

        /// <summary>Roll the customer's existing outstanding into this bill's total.</summary>
        public bool AddPreviousDuesToBill { get; set; }

        /// <summary>Amount paid at the counter (recorded as a customer receiving).</summary>
        public decimal PaidAmount { get; set; }
    }

    /// <summary>Lightweight row for the bills-history list (items not included).</summary>
    public class BillSummaryDto
    {
        public int Id { get; set; }
        public DateTime BillDate { get; set; }
        public int? ShopId { get; set; }
        /// <summary>Shop name, or the walk-in customer's name.</summary>
        public string? CustomerName { get; set; }
        public bool IsWalkIn { get; set; }
        public int? SalesmanId { get; set; }
        public string? SalesmanName { get; set; }
        public int ItemCount { get; set; }

        /// <summary>This bill's own total (excludes previous dues).</summary>
        public decimal BillTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool IsPaid { get; set; }
    }

    /// <summary>Filters for the bills list (inherits page/pageSize/search).</summary>
    public class BillQuery : PaginationQuery
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? ShopId { get; set; }
        public int? SalesmanId { get; set; }
        /// <summary>Only bills fully paid at the counter.</summary>
        public bool? PaidOnly { get; set; }
    }

    public class BillItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class BillDto
    {
        public int Id { get; set; }
        public int? ShopId { get; set; }
        public string? ShopName { get; set; }
        public int? SalesmanId { get; set; }
        public string? SalesmanName { get; set; }
        public string? WalkInCustomerName { get; set; }
        public string? WalkInCustomerPhone { get; set; }
        public DateTime BillDate { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal AdvanceTaxAmount { get; set; }
        public decimal SalesTaxAmount { get; set; }
        public decimal OtherTaxAmount { get; set; }

        /// <summary>This bill's own total (excludes previous dues). Posted to the ledger.</summary>
        public decimal BillTotal { get; set; }
        public decimal PreviousDues { get; set; }
        /// <summary>BillTotal + PreviousDues.</summary>
        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        public List<BillItemDto> Items { get; set; } = [];
    }
}
