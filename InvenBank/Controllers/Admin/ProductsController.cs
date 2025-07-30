using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenBank.API.Controllers.Admin;

/// <summary>
/// Controlador para la gestión administrativa de productos
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los productos con estadísticas
    /// </summary>
    /// <returns>Lista de productos con información de precios, stock y ventas</returns>
    /// <response code="200">Productos obtenidos exitosamente</response>
    /// <response code="401">No autorizado - Token JWT requerido</response>
    /// <response code="403">Prohibido - Requiere rol Admin</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            _logger.LogInformation("Admin solicitando todos los productos");

            var result = await _productService.GetAllProductsAsync();

            if (result.Success)
            {
                return Ok(result);
            }

            return StatusCode(500, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetAllProducts");
            return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Búsqueda avanzada de productos con filtros y paginación
    /// </summary>
    /// <param name="searchTerm">Término de búsqueda (nombre, descripción, SKU, marca)</param>
    /// <param name="categoryId">ID de categoría para filtrar</param>
    /// <param name="brand">Marca específica para filtrar</param>
    /// <param name="minPrice">Precio mínimo</param>
    /// <param name="maxPrice">Precio máximo</param>
    /// <param name="inStock">Solo productos con stock disponible</param>
    /// <param name="pageNumber">Número de página (default: 1)</param>
    /// <param name="pageSize">Tamaño de página (default: 20, max: 100)</param>
    /// <param name="sortBy">Campo para ordenar (default: Name)</param>
    /// <param name="sortDescending">Orden descendente (default: false)</param>
    /// <returns>Productos paginados con filtros aplicados</returns>
    /// <response code="200">Productos filtrados obtenidos exitosamente</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResponse<ProductDto>), 200)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? brand = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? inStock = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "Name",
        [FromQuery] bool sortDescending = false)
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
                InStock = inStock,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            _logger.LogInformation("Admin buscando productos - Término: {SearchTerm}, Página: {PageNumber}",
                searchTerm ?? "None", pageNumber);

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
    /// Obtiene un producto específico por su ID
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <returns>Producto con estadísticas completas</returns>
    /// <response code="200">Producto encontrado</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 404)]
    public async Task<IActionResult> GetProductById(int id)
    {
        try
        {
            _logger.LogInformation("Admin solicitando producto Id: {Id}", id);

            var result = await _productService.GetProductByIdAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetProductById: {Id}", id);
            return StatusCode(500, ApiResponse<ProductDto>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Obtiene el detalle completo de un producto incluyendo proveedores y especificaciones
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <returns>Detalle completo del producto con proveedores, especificaciones y productos relacionados</returns>
    /// <response code="200">Detalle del producto obtenido exitosamente</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpGet("{id:int}/detail")]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), 404)]
    public async Task<IActionResult> GetProductDetail(int id)
    {
        try
        {
            _logger.LogInformation("Admin solicitando detalle del producto Id: {Id}", id);

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
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    /// <param name="request">Datos del nuevo producto</param>
    /// <returns>Producto creado</returns>
    /// <response code="201">Producto creado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="409">Ya existe un producto con ese SKU</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 409)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<ProductDto>.ErrorResult(
                    "Datos de entrada inválidos", errors
                ));
            }

            _logger.LogInformation("Admin creando nuevo producto: {Name} - SKU: {SKU}", request.Name, request.SKU);

            var result = await _productService.CreateProductAsync(request);

            if (result.Success)
            {
                return CreatedAtAction(
                    nameof(GetProductById),
                    new { id = result.Data!.Id },
                    result
                );
            }

            // Si el error es de SKU duplicado, retornar 409 Conflict
            if (result.Message.Contains("SKU") || result.Message.Contains("existe"))
            {
                return Conflict(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CreateProduct: {Name} - SKU: {SKU}", request.Name, request.SKU);
            return StatusCode(500, ApiResponse<ProductDto>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    /// <param name="id">ID del producto a actualizar</param>
    /// <param name="request">Nuevos datos del producto</param>
    /// <returns>Producto actualizado</returns>
    /// <response code="200">Producto actualizado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Producto no encontrado</response>
    /// <response code="409">Ya existe otro producto con ese SKU</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 404)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 409)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<ProductDto>.ErrorResult(
                    "Datos de entrada inválidos", errors
                ));
            }

            _logger.LogInformation("Admin actualizando producto Id: {Id}", id);

            var result = await _productService.UpdateProductAsync(id, request);

            if (result.Success)
            {
                return Ok(result);
            }

            // Determinar el tipo de error
            if (result.Message.Contains("no encontrado"))
            {
                return NotFound(result);
            }

            if (result.Message.Contains("SKU") || result.Message.Contains("existe"))
            {
                return Conflict(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en UpdateProduct Id: {Id}", id);
            return StatusCode(500, ApiResponse<ProductDto>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Elimina un producto (soft delete)
    /// </summary>
    /// <param name="id">ID del producto a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    /// <response code="200">Producto eliminado exitosamente</response>
    /// <response code="400">No se puede eliminar - tiene dependencias (proveedores, órdenes)</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            _logger.LogInformation("Admin eliminando producto Id: {Id}", id);

            var result = await _productService.DeleteProductAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            // Determinar el tipo de error
            if (result.Message.Contains("no encontrado"))
            {
                return NotFound(result);
            }

            if (result.Message.Contains("proveedores") || result.Message.Contains("órdenes"))
            {
                return BadRequest(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en DeleteProduct Id: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Obtiene catálogo de productos (vista simplificada para clientes)
    /// </summary>
    /// <param name="categoryId">ID de categoría opcional para filtrar</param>
    /// <returns>Catálogo de productos con información básica</returns>
    /// <response code="200">Catálogo obtenido exitosamente</response>
    [HttpGet("catalog")]
    [AllowAnonymous] // Permitir acceso sin autenticación para el catálogo público
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductCatalogDto>>), 200)]
    public async Task<IActionResult> GetProductsCatalog([FromQuery] int? categoryId = null)
    {
        try
        {
            _logger.LogInformation("Solicitando catálogo de productos para categoría: {CategoryId}",
                categoryId?.ToString() ?? "Todas");

            var result = await _productService.GetProductsCatalogAsync(categoryId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetProductsCatalog");
            return StatusCode(500, ApiResponse<IEnumerable<ProductCatalogDto>>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Obtiene productos relacionados a un producto específico
    /// </summary>
    /// <param name="id">ID del producto base</param>
    /// <param name="limit">Cantidad máxima de productos relacionados (default: 5)</param>
    /// <returns>Lista de productos relacionados</returns>
    /// <response code="200">Productos relacionados obtenidos exitosamente</response>
    [HttpGet("{id:int}/related")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RelatedProductDto>>), 200)]
    public async Task<IActionResult> GetRelatedProducts(int id, [FromQuery] int limit = 5)
    {
        try
        {
            _logger.LogInformation("Solicitando productos relacionados para Id: {Id}", id);

            var result = await _productService.GetRelatedProductsAsync(id, limit);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetRelatedProducts: {Id}", id);
            return StatusCode(500, ApiResponse<IEnumerable<RelatedProductDto>>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Obtiene estadísticas generales de productos
    /// </summary>
    /// <returns>Estadísticas completas del inventario de productos</returns>
    /// <response code="200">Estadísticas obtenidas exitosamente</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<ProductStatsDto>), 200)]
    public async Task<IActionResult> GetProductStats()
    {
        try
        {
            _logger.LogInformation("Admin solicitando estadísticas de productos");

            var result = await _productService.GetProductStatsAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetProductStats");
            return StatusCode(500, ApiResponse<ProductStatsDto>.ErrorResult(
                "Error interno del servidor"
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
            _logger.LogInformation("Solicitando marcas de productos");

            var result = await _productService.GetBrandsAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetBrands");
            return StatusCode(500, ApiResponse<IEnumerable<string>>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Obtiene productos por categoría específica
    /// </summary>
    /// <param name="categoryId">ID de la categoría</param>
    /// <returns>Lista de productos de la categoría</returns>
    /// <response code="200">Productos por categoría obtenidos exitosamente</response>
    [HttpGet("by-category/{categoryId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        try
        {
            _logger.LogInformation("Solicitando productos por categoría: {CategoryId}", categoryId);

            var result = await _productService.GetProductsByCategoryAsync(categoryId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetProductsByCategory: {CategoryId}", categoryId);
            return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Obtiene productos por marca específica
    /// </summary>
    /// <param name="brand">Nombre de la marca</param>
    /// <returns>Lista de productos de la marca</returns>
    /// <response code="200">Productos por marca obtenidos exitosamente</response>
    [HttpGet("by-brand/{brand}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
    public async Task<IActionResult> GetProductsByBrand(string brand)
    {
        try
        {
            _logger.LogInformation("Solicitando productos por marca: {Brand}", brand);

            var result = await _productService.GetProductsByBrandAsync(brand);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetProductsByBrand: {Brand}", brand);
            return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }
}