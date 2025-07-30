namespace InvenBank.API.DTOs
{
    public class SupplierDto : ActiveDto
    {
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? TaxId { get; set; }
        public int ProductCount { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
