namespace InvenBank.API.DTOs
{
    public class ProductSupplierDto : ActiveDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? BatchNumber { get; set; }
        public string? SupplierSKU { get; set; }
        public DateTime? LastRestockDate { get; set; }
    }
}
