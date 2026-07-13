using System.ComponentModel.DataAnnotations;

namespace DMS_Backend.Contracts
{
    public class ReturnItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }

    /// <summary>Records a customer return, attributed to the salesman responsible.</summary>
    public class CreateReturnRequest
    {
        [Required]
        public int ShopId { get; set; }

        /// <summary>The salesman this return belongs to (required).</summary>
        [Required]
        public int SalesmanId { get; set; }

        [Required]
        public decimal ReturnAmount { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        [Required(ErrorMessage = "A description of the returned items is required.")]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>Optional. Returned products are put back into stock.</summary>
        public List<ReturnItemRequest> Items { get; set; } = [];
    }

    public class ReturnDto
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string? ShopName { get; set; }
        public int? SalesmanId { get; set; }
        public string? SalesmanName { get; set; }
        public decimal ReturnAmount { get; set; }
        public DateTime ReturnDate { get; set; }
        public string? Description { get; set; }
        /// <summary>Snapshot of products returned, e.g. "Pepsi 1.5L(12), Juice(6)".</summary>
        public string? ReturnProducts { get; set; }
    }

    /// <summary>Filters for listing returns (inherits page/pageSize/search).</summary>
    public class ReturnQuery : PaginationQuery
    {
        public int? SalesmanId { get; set; }
        public int? ShopId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
