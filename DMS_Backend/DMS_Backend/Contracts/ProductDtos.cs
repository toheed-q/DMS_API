namespace DMS_Backend.Contracts
{
    /// <summary>
    /// Product as returned by the API. The image blob is deliberately excluded
    /// (only a HasImage flag) so lists stay lightweight and fast — the image is
    /// fetched separately on demand.
    /// </summary>
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Size { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal CompanyPrice { get; set; }
        public decimal? RetailPrice { get; set; }
        public decimal? TradePrice { get; set; }
        public int? AvailableStock { get; set; }
        public bool HasImage { get; set; }
    }
}
