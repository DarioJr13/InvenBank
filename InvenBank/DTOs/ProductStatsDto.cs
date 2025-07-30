namespace InvenBank.API.DTOs
{
    // ===============================================
    // DTOs PARA ESTADÍSTICAS
    // ===============================================

    public class ProductStatsDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int ProductsInStock { get; set; }
        public int ProductsLowStock { get; set; }
        public int ProductsOutOfStock { get; set; }
        public int TotalCategories { get; set; }
        public int TotalBrands { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public ProductDto? TopSellingProduct { get; set; }
        public ProductDto? MostWishedProduct { get; set; }
        public List<CategoryProductCount> ProductsByCategory { get; set; } = new();
        public List<BrandProductCount> ProductsByBrand { get; set; } = new();
    }
}
