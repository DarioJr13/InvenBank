namespace InvenBank.API.DTOs.Requests
{
    // ===============================================
    // DTOs PARA BÚSQUEDA AVANZADA
    // ===============================================

    public class ProductSearchRequest : PaginationRequest
    {
        public int? CategoryId { get; set; }
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; } = true;
        public bool? IsAvailable { get; set; } = true;
        public string? StockStatus { get; set; } // "Disponible", "Stock Bajo", "Sin Stock"
    }
}
