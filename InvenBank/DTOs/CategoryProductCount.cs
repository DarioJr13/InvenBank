namespace InvenBank.API.DTOs
{
    public class CategoryProductCount
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal TotalInventoryValue { get; set; }
    }
}
