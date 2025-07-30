using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;

namespace InvenBank.API.Services.Interfaces
{
    public interface IProductService
    {
        // ===============================================
        // CRUD BÁSICO
        // ===============================================
        Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync();
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<ProductDetailDto>> GetProductDetailAsync(int id);
        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductRequest request);
        Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductRequest request);
        Task<ApiResponse<bool>> DeleteProductAsync(int id);

        // ===============================================
        // BÚSQUEDA Y FILTRADO
        // ===============================================
        Task<PagedResponse<ProductDto>> GetProductsSearchAsync(ProductSearchRequest request);
        Task<ApiResponse<IEnumerable<ProductCatalogDto>>> GetProductsCatalogAsync(int? categoryId = null);

        // ===============================================
        // CONSULTAS ESPECIALIZADAS
        // ===============================================
        Task<ApiResponse<IEnumerable<RelatedProductDto>>> GetRelatedProductsAsync(int productId, int limit = 5);
        Task<ApiResponse<ProductStatsDto>> GetProductStatsAsync();
        Task<ApiResponse<IEnumerable<string>>> GetBrandsAsync();
        Task<ApiResponse<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(int categoryId);
        Task<ApiResponse<IEnumerable<ProductDto>>> GetProductsByBrandAsync(string brand);
    }
}