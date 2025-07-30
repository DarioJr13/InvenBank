using AutoMapper;
using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Entities;
using InvenBank.API.Repositories.Interfaces;
using InvenBank.API.Services.Interfaces;
using System.Text.Json;

namespace InvenBank.API.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // ===============================================
        // OBTENER TODOS LOS PRODUCTOS
        // ===============================================

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los productos con estadísticas");

                var products = await _productRepository.GetProductsWithStatsAsync();

                return ApiResponse<IEnumerable<ProductDto>>.SuccessResult(
                    products,
                    $"Se obtuvieron {products.Count()} productos exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los productos");
                return ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                    "Error al obtener los productos"
                );
            }
        }

        // ===============================================
        // OBTENER PRODUCTO POR ID
        // ===============================================

        public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo producto por Id: {Id}", id);

                var product = await _productRepository.GetProductWithStatsAsync(id);

                if (product == null)
                {
                    _logger.LogWarning("Producto no encontrado: {Id}", id);
                    return ApiResponse<ProductDto>.ErrorResult("Producto no encontrado");
                }

                return ApiResponse<ProductDto>.SuccessResult(
                    product,
                    "Producto obtenido exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto por Id: {Id}", id);
                return ApiResponse<ProductDto>.ErrorResult("Error al obtener el producto");
            }
        }

        // ===============================================
        // OBTENER DETALLE COMPLETO DEL PRODUCTO
        // ===============================================

        public async Task<ApiResponse<ProductDetailDto>> GetProductDetailAsync(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo detalle completo del producto Id: {Id}", id);

                var productDetail = await _productRepository.GetProductDetailAsync(id);

                if (productDetail == null)
                {
                    _logger.LogWarning("Producto no encontrado para detalle: {Id}", id);
                    return ApiResponse<ProductDetailDto>.ErrorResult("Producto no encontrado");
                }

                return ApiResponse<ProductDetailDto>.SuccessResult(
                    productDetail,
                    "Detalle del producto obtenido exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle del producto Id: {Id}", id);
                return ApiResponse<ProductDetailDto>.ErrorResult("Error al obtener el detalle del producto");
            }
        }

        // ===============================================
        // CREAR NUEVO PRODUCTO ✅ CON MAPEOS COMPLETOS
        // ===============================================

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductRequest request)
        {
            try
            {
                _logger.LogInformation("Creando nuevo producto: {Name} - SKU: {SKU}", request.Name, request.SKU);

                // Validar que el SKU no exista
                var skuExists = await _productRepository.ExistsBySKUAsync(request.SKU);
                if (skuExists)
                {
                    return ApiResponse<ProductDto>.ErrorResult(
                        "Ya existe un producto con ese SKU"
                    );
                }

                // Validar que la categoría exista
                var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
                if (!categoryExists)
                {
                    return ApiResponse<ProductDto>.ErrorResult(
                        "La categoría especificada no existe"
                    );
                }

                // Validar especificaciones JSON si existe
                if (!string.IsNullOrWhiteSpace(request.Specifications))
                {
                    if (!IsValidJson(request.Specifications))
                    {
                        return ApiResponse<ProductDto>.ErrorResult(
                            "Las especificaciones deben estar en formato JSON válido"
                        );
                    }
                }

                // ✅ USAR AUTOMAPPER PARA MAPEAR CreateProductRequest -> Product
                var product = _mapper.Map<Product>(request);

                // Asegurar valores por defecto
                product.IsActive = true;
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                var productId = await _productRepository.CreateAsync(product);

                // Obtener el producto creado con estadísticas
                var createdProduct = await _productRepository.GetProductWithStatsAsync(productId);

                _logger.LogInformation("Producto creado exitosamente: {Id} - {Name}", productId, request.Name);

                return ApiResponse<ProductDto>.SuccessResult(
                    createdProduct!,
                    "Producto creado exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto: {Name} - SKU: {SKU}", request.Name, request.SKU);
                return ApiResponse<ProductDto>.ErrorResult("Error al crear el producto");
            }
        }

        // ===============================================
        // ACTUALIZAR PRODUCTO ✅ CON MAPEOS COMPLETOS
        // ===============================================

        public async Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductRequest request)
        {
            try
            {
                _logger.LogInformation("Actualizando producto Id: {Id}", id);

                // Verificar que el producto existe
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    return ApiResponse<ProductDto>.ErrorResult("Producto no encontrado");
                }

                // Validar que el SKU no esté duplicado
                var skuExists = await _productRepository.ExistsBySKUAsync(request.SKU, id);
                if (skuExists)
                {
                    return ApiResponse<ProductDto>.ErrorResult(
                        "Ya existe otro producto con ese SKU"
                    );
                }

                // Validar que la categoría exista
                var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
                if (!categoryExists)
                {
                    return ApiResponse<ProductDto>.ErrorResult(
                        "La categoría especificada no existe"
                    );
                }

                // Validar especificaciones JSON si existe
                if (!string.IsNullOrWhiteSpace(request.Specifications))
                {
                    if (!IsValidJson(request.Specifications))
                    {
                        return ApiResponse<ProductDto>.ErrorResult(
                            "Las especificaciones deben estar en formato JSON válido"
                        );
                    }
                }

                // ✅ USAR AUTOMAPPER PARA MAPEAR UpdateProductRequest -> Product existente
                _mapper.Map(request, existingProduct);

                // Mantener campos críticos
                existingProduct.Id = id;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                // Actualizar
                var success = await _productRepository.UpdateAsync(existingProduct);
                if (!success)
                {
                    return ApiResponse<ProductDto>.ErrorResult("No se pudo actualizar el producto");
                }

                // Obtener el producto actualizado
                var updatedProduct = await _productRepository.GetProductWithStatsAsync(id);

                _logger.LogInformation("Producto actualizado exitosamente: {Id}", id);

                return ApiResponse<ProductDto>.SuccessResult(
                    updatedProduct!,
                    "Producto actualizado exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto Id: {Id}", id);
                return ApiResponse<ProductDto>.ErrorResult("Error al actualizar el producto");
            }
        }

        // ===============================================
        // ELIMINAR PRODUCTO (SOFT DELETE)
        // ===============================================

        public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando producto Id: {Id}", id);

                // Verificar que el producto existe
                var exists = await _productRepository.ExistsAsync(id);
                if (!exists)
                {
                    return ApiResponse<bool>.ErrorResult("Producto no encontrado");
                }

                // Verificar que no tenga proveedores asociados
                var hasSuppliers = await _productRepository.HasSuppliersAsync(id);
                if (hasSuppliers)
                {
                    return ApiResponse<bool>.ErrorResult(
                        "No se puede eliminar un producto que tiene proveedores asociados"
                    );
                }

                // Verificar que no tenga órdenes asociadas
                var hasOrders = await _productRepository.HasOrdersAsync(id);
                if (hasOrders)
                {
                    return ApiResponse<bool>.ErrorResult(
                        "No se puede eliminar un producto que tiene órdenes asociadas"
                    );
                }

                // Realizar soft delete
                var success = await _productRepository.DeleteAsync(id);

                if (success)
                {
                    _logger.LogInformation("Producto eliminado exitosamente: {Id}", id);
                    return ApiResponse<bool>.SuccessResult(true, "Producto eliminado exitosamente");
                }
                else
                {
                    return ApiResponse<bool>.ErrorResult("No se pudo eliminar el producto");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto Id: {Id}", id);
                return ApiResponse<bool>.ErrorResult("Error al eliminar el producto");
            }
        }

        // ===============================================
        // BÚSQUEDA AVANZADA CON FILTROS
        // ===============================================

        public async Task<PagedResponse<ProductDto>> GetProductsSearchAsync(ProductSearchRequest request)
        {
            try
            {
                _logger.LogInformation("Búsqueda de productos - Término: {SearchTerm}, Página: {PageNumber}",
                    request.SearchTerm ?? "None", request.PageNumber);

                var result = await _productRepository.GetProductsSearchAsync(request);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de productos");
                return PagedResponse<ProductDto>.Create(
                    Enumerable.Empty<ProductDto>(),
                    request.PageNumber,
                    request.PageSize,
                    0
                );
            }
        }

        // ===============================================
        // CATÁLOGO DE PRODUCTOS PARA CLIENTES ✅
        // ===============================================

        public async Task<ApiResponse<IEnumerable<ProductCatalogDto>>> GetProductsCatalogAsync(int? categoryId = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo catálogo de productos para categoría: {CategoryId}",
                    categoryId?.ToString() ?? "Todas");

                var products = await _productRepository.GetProductsCatalogAsync(categoryId);

                return ApiResponse<IEnumerable<ProductCatalogDto>>.SuccessResult(
                    products,
                    $"Catálogo obtenido con {products.Count()} productos"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener catálogo de productos");
                return ApiResponse<IEnumerable<ProductCatalogDto>>.ErrorResult(
                    "Error al obtener el catálogo de productos"
                );
            }
        }

        // ===============================================
        // PRODUCTOS RELACIONADOS ✅
        // ===============================================

        public async Task<ApiResponse<IEnumerable<RelatedProductDto>>> GetRelatedProductsAsync(int productId, int limit = 5)
        {
            try
            {
                _logger.LogInformation("Obteniendo productos relacionados para Id: {ProductId}", productId);

                var relatedProducts = await _productRepository.GetRelatedProductsAsync(productId, limit);

                return ApiResponse<IEnumerable<RelatedProductDto>>.SuccessResult(
                    relatedProducts,
                    $"Se encontraron {relatedProducts.Count()} productos relacionados"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos relacionados para Id: {ProductId}", productId);
                return ApiResponse<IEnumerable<RelatedProductDto>>.ErrorResult(
                    "Error al obtener productos relacionados"
                );
            }
        }

        // ===============================================
        // ESTADÍSTICAS DE PRODUCTOS
        // ===============================================

        public async Task<ApiResponse<ProductStatsDto>> GetProductStatsAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas de productos");

                var stats = await _productRepository.GetProductStatsAsync();

                return ApiResponse<ProductStatsDto>.SuccessResult(
                    stats,
                    "Estadísticas obtenidas exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de productos");
                return ApiResponse<ProductStatsDto>.ErrorResult(
                    "Error al obtener las estadísticas de productos"
                );
            }
        }

        // ===============================================
        // OBTENER MARCAS
        // ===============================================

        public async Task<ApiResponse<IEnumerable<string>>> GetBrandsAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo marcas de productos");

                var brands = await _productRepository.GetBrandsAsync();

                return ApiResponse<IEnumerable<string>>.SuccessResult(
                    brands,
                    $"Se obtuvieron {brands.Count()} marcas"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener marcas de productos");
                return ApiResponse<IEnumerable<string>>.ErrorResult(
                    "Error al obtener las marcas"
                );
            }
        }

        // ===============================================
        // PRODUCTOS POR CATEGORÍA ✅ CON MAPEOS
        // ===============================================

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                _logger.LogInformation("Obteniendo productos por categoría: {CategoryId}", categoryId);

                // El repositorio ya devuelve ProductDto, no necesitamos mapear
                var products = await _productRepository.GetProductsByCategoryAsync(categoryId);

                return ApiResponse<IEnumerable<ProductDto>>.SuccessResult(
                    products,
                    $"Se obtuvieron {products.Count()} productos de la categoría"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por categoría: {CategoryId}", categoryId);
                return ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                    "Error al obtener productos de la categoría"
                );
            }
        }

        // ===============================================
        // PRODUCTOS POR MARCA ✅ CON MAPEOS
        // ===============================================

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetProductsByBrandAsync(string brand)
        {
            try
            {
                _logger.LogInformation("Obteniendo productos por marca: {Brand}", brand);

                // El repositorio ya devuelve ProductDto, no necesitamos mapear
                var products = await _productRepository.GetProductsByBrandAsync(brand);

                return ApiResponse<IEnumerable<ProductDto>>.SuccessResult(
                    products,
                    $"Se obtuvieron {products.Count()} productos de la marca {brand}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por marca: {Brand}", brand);
                return ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                    "Error al obtener productos de la marca"
                );
            }
        }

        // ===============================================
        // MÉTODOS ADICIONALES PARA IMPORTACIÓN/EXPORTACIÓN
        // ===============================================

        public async Task<ApiResponse<IEnumerable<ProductDto>>> ImportProductsAsync(IEnumerable<ProductImportDto> importData)
        {
            try
            {
                _logger.LogInformation("Importando {Count} productos", importData.Count());

                var importedProducts = new List<ProductDto>();

                foreach (var importItem in importData)
                {
                    // Buscar la categoría por nombre
                    var categories = await _categoryRepository.GetAllAsync();
                    var category = categories.FirstOrDefault(c => c.Name.Equals(importItem.CategoryName, StringComparison.OrdinalIgnoreCase));

                    if (category == null)
                    {
                        _logger.LogWarning("Categoría no encontrada para importación: {CategoryName}", importItem.CategoryName);
                        continue;
                    }

                    // ✅ USAR AUTOMAPPER PARA MAPEAR ProductImportDto -> Product
                    var product = _mapper.Map<Product>(importItem);
                    product.CategoryId = category.Id;

                    var productId = await _productRepository.CreateAsync(product);
                    var createdProduct = await _productRepository.GetProductWithStatsAsync(productId);

                    if (createdProduct != null)
                    {
                        importedProducts.Add(createdProduct);
                    }
                }

                return ApiResponse<IEnumerable<ProductDto>>.SuccessResult(
                    importedProducts,
                    $"Se importaron {importedProducts.Count} productos exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al importar productos");
                return ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                    "Error al importar productos"
                );
            }
        }

        public async Task<ApiResponse<IEnumerable<ProductExportDto>>> ExportProductsAsync()
        {
            try
            {
                _logger.LogInformation("Exportando productos");

                var products = await _productRepository.GetAllAsync();

                // ✅ USAR AUTOMAPPER PARA MAPEAR Product -> ProductExportDto
                var exportData = _mapper.Map<IEnumerable<ProductExportDto>>(products);

                return ApiResponse<IEnumerable<ProductExportDto>>.SuccessResult(
                    exportData,
                    $"Se exportaron {exportData.Count()} productos exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar productos");
                return ApiResponse<IEnumerable<ProductExportDto>>.ErrorResult(
                    "Error al exportar productos"
                );
            }
        }

        // ===============================================
        // MÉTODOS DE UTILIDAD PRIVADOS
        // ===============================================

        private bool IsValidJson(string jsonString)
        {
            try
            {
                JsonDocument.Parse(jsonString);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}