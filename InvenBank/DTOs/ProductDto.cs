namespace InvenBank.API.DTOs
{
    public class ProductDto : ActiveDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SKU { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryPath { get; set; } // Para jerarquía completa
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public string? Specifications { get; set; } // JSON format

        // Estadísticas de precios y stock (desde ProductSuppliers)
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal? BestPrice { get; set; }
        public int TotalStock { get; set; }
        public int SupplierCount { get; set; }

        // Estadísticas de ventas
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int WishlistCount { get; set; }

        // Estado del producto
        public string StockStatus { get; set; } = string.Empty; // "Disponible", "Stock Bajo", "Sin Stock"
        public bool IsAvailable { get; set; }
        public DateTime? LastRestockDate { get; set; }
    }
}
