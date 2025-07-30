using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;

namespace InvenBank.API.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de productos con funcionalidades completas
    /// </summary>
    public interface IProductService
    {
        // ===============================================
        // CRUD BÁSICO
        // ===============================================

        /// <summary>
        /// Obtiene todos los productos con estadísticas
        /// </summary>
        Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync();

        /// <summary>
        /// Obtiene un producto por su ID con estadísticas
        /// </summary>
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id);

        /// <summary>
        /// Obtiene el detalle completo de un producto (incluye proveedores, especificaciones, relacionados)
        /// </summary>
        Task<ApiResponse<ProductDetailDto>> GetProductDetailAsync(int id);

        /// <summary>
        /// Crea un nuevo producto
        /// </summary>
        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductRequest request);

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductRequest request);

        /// <summary>
        /// Elimina un producto (soft delete)
        /// </summary>
        Task<ApiResponse<bool>> DeleteProductAsync(int id);

        // ===============================================
        // BÚSQUEDA Y FILTRADO
        // ===============================================

        /// <summary>
        /// Búsqueda avanzada de productos con filtros y paginación
        /// </summary>
        Task<PagedResponse<ProductDto>> GetProductsSearchAsync(ProductSearchRequest request);

        /// <summary>
        /// Obtiene el catálogo de productos para clientes (vista simplificada)
        /// </summary>
        Task<ApiResponse<IEnumerable<ProductCatalogDto>>> GetProductsCatalogAsync(int? categoryId = null);

        // ===============================================
        // CONSULTAS ESPECIALIZADAS
        // ===============================================

        /// <summary>
        /// Obtiene productos relacionados a un producto específico
        /// </summary>
        Task<ApiResponse<IEnumerable<RelatedProductDto>>> GetRelatedProductsAsync(int productId, int limit = 5);

        /// <summary>
        /// Obtiene estadísticas generales de productos
        /// </summary>
        Task<ApiResponse<ProductStatsDto>> GetProductStatsAsync();

        /// <summary>
        /// Obtiene todas las marcas disponibles
        /// </summary>
        Task<ApiResponse<IEnumerable<string>>> GetBrandsAsync();

        /// <summary>
        /// Obtiene productos por categoría específica
        /// </summary>
        Task<ApiResponse<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(int categoryId);

        /// <summary>
        /// Obtiene productos por marca específica
        /// </summary>
        Task<ApiResponse<IEnumerable<ProductDto>>> GetProductsByBrandAsync(string brand);

        // ===============================================
        // IMPORTACIÓN Y EXPORTACIÓN
        // ===============================================

        /// <summary>
        /// Importa productos desde datos externos (CSV, Excel, etc.)
        /// </summary>
        Task<ApiResponse<IEnumerable<ProductDto>>> ImportProductsAsync(IEnumerable<ProductImportDto> importData);

        /// <summary>
        /// Exporta productos a formato de exportación
        /// </summary>
        Task<ApiResponse<IEnumerable<ProductExportDto>>> ExportProductsAsync();
    }
}