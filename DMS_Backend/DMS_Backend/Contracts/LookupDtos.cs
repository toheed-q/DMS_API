namespace DMS_Backend.Contracts
{
    // Reference/lookup data used to populate dropdowns and filters.

    public class RouteDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ShopCount { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public string? CompanyName { get; set; }
        public int ProductCount { get; set; }
    }

    public class SalesmanDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? CNIC { get; set; }
        public decimal MonthlySalary { get; set; }
        public string? IncentiveType { get; set; }
        public decimal? IncentiveValue { get; set; }
        public bool IsActive { get; set; }
    }
}
