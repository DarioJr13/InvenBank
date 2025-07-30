using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Repositories.Interfaces;
using InvenBank.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenBank.API.Controllers.Client;

/// <summary>
/// Controlador de catálogo para la aplicación móvil de clientes
/// </summary>
[ApiController]
[Route("api/client/[controller]")]
[Produces("application/json")]
public class CatalogController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CatalogController> _logger;

    public CatalogController(
        IProductService productService,
        ICategoryService categoryService,
        ILogger<CatalogController> logger)
    {
        _productService = productService;
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el catálogo completo de productos disponibles
    /// </summary>
    /// <param name="categoryId">ID de categoría opcional para filtrar</param>
    /// <returns>Catálogo de productos con información básica para clientes</returns>
    /// <response code="200">Catálogo obtenido exitosamente</response>
    [HttpGet("products")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductCatalogDto>>), 200)]
    public async Task<IActionResult> GetCatalog([FromQuery] int? categoryId = null)
    {
        try
        {
            _logger.LogInformation("Cliente solicitando catálogo - Categoría: {CategoryId}",
                categoryId?.ToString() ?? "Todas");

            var result = await _productService.GetProductsCatalogAsync(categoryId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetCatalog");
            return StatusCode(500, ApiResponse<IEnumerable<ProductCatalogDto>>.ErrorResult(
                "Error al obtener el catálogo"
            ));
        }
    }

    /// <summary>
    /// Búsqueda de productos para clientes con filtros básicos
    /// </summary>
    /// <param name="searchTerm">Término de búsqueda</param>
    /// <param name="categoryId">ID de categoría</param>
    /// <param name="brand">Marca específica</param>
    /// <param name="minPrice">Precio mínimo</param>
    /// <param name="maxPrice">Precio máximo</param>
    /// <param name="pageNumber">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Productos que coinciden con los criterios de búsqueda</returns>
    /// <response code="200">Búsqueda realizada exitosamente</response>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResponse<ProductDto>), 200)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? brand = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var request = new ProductSearchRequest
            {
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                Brand = brand,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                InStock = true, // Solo productos disponibles para clientes
                IsAvailable = true,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = "Name",
                SortDescending = false
            };

            _logger.LogInformation("Cliente buscando productos - Término: {SearchTerm}",
                searchTerm ?? "None");

            var result = await _productService.GetProductsSearchAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en SearchProducts");
            return StatusCode(500, PagedResponse<ProductDto>.Create(
                Enumerable.Empty<ProductDto>(), pageNumber, pageSize, 0
            ));
        }
    }

    /// <summary>
    /// Obtiene el detalle completo de un producto específico
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <returns>Detalle completo del producto con proveedores y precios</returns>
    /// <response code="200">Detalle del producto obtenido exitosamente</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpGet("products/{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), 404)]
    public async Task<IActionResult> GetProductDetail(int id)
    {
        try
        {
            _logger.LogInformation("Cliente solicitando detalle del producto Id: {Id}", id);

            var result = await _productService.GetProductDetailAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetProductDetail: {Id}", id);
            return StatusCode(500, ApiResponse<ProductDetailDto>.ErrorResult(
                "Error al obtener el detalle del producto"
            ));
        }
    }

    /// <summary>
    /// Obtiene productos relacionados a un producto específico
    /// </summary>
    /// <param name="id">ID del producto base</param>
    /// <param name="limit">Cantidad máxima de productos relacionados</param>
    /// <returns>Lista de productos relacionados</returns>
    /// <response code="200">Productos relacionados obtenidos exitosamente</response>
    [HttpGet("products/{id:int}/related")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RelatedProductDto>>), 200)]
    public async Task<IActionResult> GetRelatedProducts(int id, [FromQuery] int limit = 5)
    {
        try
        {
            _logger.LogInformation("Cliente solicitando productos relacionados para Id: {Id}", id);

            var result = await _productService.GetRelatedProductsAsync(id, limit);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetRelatedProducts: {Id}", id);
            return StatusCode(500, ApiResponse<IEnumerable<RelatedProductDto>>.ErrorResult(
                "Error al obtener productos relacionados"
            ));
        }
    }

    /// <summary>
    /// Obtiene todas las categorías disponibles
    /// </summary>
    /// <returns>Lista de categorías con jerarquía</returns>
    /// <response code="200">Categorías obtenidas exitosamente</response>
    [HttpGet("categories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), 200)]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            _logger.LogInformation("Cliente solicitando categorías");

            var result = await _categoryService.GetCategoryHierarchyAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetCategories");
            return StatusCode(500, ApiResponse<IEnumerable<CategoryDto>>.ErrorResult(
                "Error al obtener las categorías"
            ));
        }
    }

    /// <summary>
    /// Obtiene productos por categoría específica
    /// </summary>
    /// <param name="categoryId">ID de la categoría</param>
    /// <param name="pageNumber">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Productos de la categoría especificada</returns>
    /// <response code="200">Productos por categoría obtenidos exitosamente</response>
    [HttpGet("categories/{categoryId:int}/products")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResponse<ProductDto>), 200)]
    public async Task<IActionResult> GetProductsByCategory(
        int categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Cliente solicitando productos por categoría: {CategoryId}", categoryId);

            var request = new ProductSearchRequest
            {
                CategoryId = categoryId,
                InStock = true,
                IsAvailable = true,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = "Name"
            };

            var result = await _productService.GetProductsSearchAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetProductsByCategory: {CategoryId}", categoryId);
            return StatusCode(500, PagedResponse<ProductDto>.Create(
                Enumerable.Empty<ProductDto>(), pageNumber, pageSize, 0
            ));
        }
    }

    /// <summary>
    /// Obtiene todas las marcas disponibles
    /// </summary>
    /// <returns>Lista de marcas únicas</returns>
    /// <response code="200">Marcas obtenidas exitosamente</response>
    [HttpGet("brands")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), 200)]
    public async Task<IActionResult> GetBrands()
    {
        try
        {
            _logger.LogInformation("Cliente solicitando marcas");

            var result = await _productService.GetBrandsAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetBrands");
            return StatusCode(500, ApiResponse<IEnumerable<string>>.ErrorResult(
                "Error al obtener las marcas"
            ));
        }
    }

    /// <summary>
    /// Obtiene productos por marca específica
    /// </summary>
    /// <param name="brand">Nombre de la marca</param>
    /// <param name="pageNumber">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Productos de la marca especificada</returns>
    /// <response code="200">Productos por marca obtenidos exitosamente</response>
    [HttpGet("brands/{brand}/products")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResponse<ProductDto>), 200)]
    public async Task<IActionResult> GetProductsByBrand(
        string brand,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Cliente solicitando productos por marca: {Brand}", brand);

            var request = new ProductSearchRequest
            {
                Brand = brand,
                InStock = true,
                IsAvailable = true,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = "Name"
            };

            var result = await _productService.GetProductsSearchAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetProductsByBrand: {Brand}", brand);
            return StatusCode(500, PagedResponse<ProductDto>.Create(
                Enumerable.Empty<ProductDto>(), pageNumber, pageSize, 0
            ));
        }
    }

    /// <summary>
    /// Obtiene productos populares/destacados
    /// </summary>
    /// <param name="limit">Cantidad máxima de productos</param>
    /// <returns>Lista de productos más vendidos</returns>
    /// <response code="200">Productos populares obtenidos exitosamente</response>
    [HttpGet("featured")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
    public async Task<IActionResult> GetFeaturedProducts([FromQuery] int limit = 10)
    {
        try
        {
            _logger.LogInformation("Cliente solicitando productos destacados");

            // Búsqueda de productos disponibles ordenados por popularidad
            var request = new ProductSearchRequest
            {
                InStock = true,
                IsAvailable = true,
                PageNumber = 1,
                PageSize = limit,
                SortBy = "TotalSold", // Ordenar por más vendidos
                SortDescending = true
            };

            var result = await _productService.GetProductsSearchAsync(request);

            // Convertir a respuesta simple
            var featuredProducts = result.Data ?? Enumerable.Empty<ProductDto>();

            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(
                featuredProducts,
                $"Se obtuvieron {featuredProducts.Count()} productos destacados"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetFeaturedProducts");
            return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                "Error al obtener productos destacados"
            ));
        }
    }

    /// <summary>
    /// Obtiene ofertas especiales (productos con descuento o precios bajos)
    /// </summary>
    /// <param name="limit">Cantidad máxima de ofertas</param>
    /// <returns>Lista de productos en oferta</returns>
    /// <response code="200">Ofertas obtenidas exitosamente</response>
    [HttpGet("deals")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
    public async Task<IActionResult> GetDeals([FromQuery] int limit = 10)
    {
        try
        {
            _logger.LogInformation("Cliente solicitando ofertas especiales");

            // Búsqueda de productos disponibles ordenados por precio más bajo
            var request = new ProductSearchRequest
            {
                InStock = true,
                IsAvailable = true,
                PageNumber = 1,
                PageSize = limit,
                SortBy = "MinPrice", // Ordenar por precio más bajo
                SortDescending = false
            };

            var result = await _productService.GetProductsSearchAsync(request);

            // Convertir a respuesta simple
            var deals = result.Data ?? Enumerable.Empty<ProductDto>();

            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(
                deals,
                $"Se obtuvieron {deals.Count()} ofertas especiales"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetDeals");
            return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                "Error al obtener ofertas especiales"
            ));
        }
    }
}