namespace DMS_Backend.Contracts
{
    public class ShopDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? NTNNumber { get; set; }
        public string? FBRNumber { get; set; }
        public int? RouteId { get; set; }
        public string? RouteName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
