namespace InvenBank.API.DTOs.Requests
{
    public class CreateProductSupplierRequest
    {
        public int ProductId { get; set; }
        public int SupplierId { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? BatchNumber { get; set; }
        public string? SupplierSKU { get; set; }
        public DateTime? LastRestockDate { get; set; }
    }
}
