namespace InvenBank.API.DTOs
{
    public class BrandProductCount
    {
        public string Brand { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal AveragePrice { get; set; }
        public int TotalStock { get; set; }
    }
}
