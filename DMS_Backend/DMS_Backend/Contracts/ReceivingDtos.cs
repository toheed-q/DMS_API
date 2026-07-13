using System.ComponentModel.DataAnnotations;

namespace DMS_Backend.Contracts
{
    /// <summary>Payload to record a customer-level payment (receiving) against a shop.</summary>
    public class CreateReceivingRequest
    {
        [Required]
        public int ShopId { get; set; }

        /// <summary>Money received. Must be greater than zero.</summary>
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime ReceivedDate { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }

    public class ReceivingDto
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string? ShopName { get; set; }
        public decimal Amount { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
