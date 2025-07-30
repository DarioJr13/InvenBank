namespace InvenBank.API.DTOs
{
    public class ProductCatalogDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool IsAvailable { get; set; }
        public string StockStatus { get; set; } = string.Empty;
        public int SupplierCount { get; set; }
    }
}
