namespace InvenBank.API.DTOs
{
    public class ProductSupplierInfo
    {
        public int ProductSupplierId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? BatchNumber { get; set; }
        public string? SupplierSKU { get; set; }
        public DateTime? LastRestockDate { get; set; }
        public bool IsActive { get; set; }
    }
}
