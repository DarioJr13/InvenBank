namespace InvenBank.API.DTOs
{
    // ===============================================
    // DTOs PARA IMPORTACIÓN/EXPORTACIÓN
    // ===============================================

    public class ProductImportDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty; // Se convierte a CategoryId
        public string? Brand { get; set; }
        public string? ImageUrl { get; set; }
        public string? Specifications { get; set; }
    }
}
