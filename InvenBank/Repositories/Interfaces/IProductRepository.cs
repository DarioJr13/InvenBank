using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Entities;

namespace InvenBank.API.Repositories.Interfaces
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<IEnumerable<ProductDto>> GetProductsWithStatsAsync();
        Task<ProductDto?> GetProductWithStatsAsync(int id);
        Task<ProductDetailDto?> GetProductDetailAsync(int id);
        Task<PagedResponse<ProductDto>> GetProductsSearchAsync(ProductSearchRequest request);
        Task<IEnumerable<ProductCatalogDto>> GetProductsCatalogAsync(int? categoryId = null);
        Task<IEnumerable<RelatedProductDto>> GetRelatedProductsAsync(int productId, int limit = 5);
        Task<bool> ExistsBySKUAsync(string sku, int? excludeId = null);
        Task<bool> HasSuppliersAsync(int productId);
        Task<bool> HasWishlistEntriesAsync(int productId);
        Task<bool> HasOrdersAsync(int productId);
        Task<ProductStatsDto> GetProductStatsAsync();
        Task<IEnumerable<string>> GetBrandsAsync();
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductDto>> GetProductsByBrandAsync(string brand);
    }
}
