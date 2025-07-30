namespace InvenBank.API.DTOs
{
    public class ProductDetailDto : ProductDto
    {
        // Información extendida para vista de detalle
        public List<ProductSupplierInfo> Suppliers { get; set; } = new();
        public List<ProductSpecificationItem> ParsedSpecifications { get; set; } = new();
        public List<RelatedProductDto> RelatedProducts { get; set; } = new();
    }
}
