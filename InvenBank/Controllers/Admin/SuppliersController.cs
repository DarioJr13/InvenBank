using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenBank.API.Controllers.Admin;

/// <summary>
/// Controlador para la gestión administrativa de proveedores
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(
        ISupplierService supplierService,
        ILogger<SuppliersController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los proveedores con estadísticas
    /// </summary>
    /// <returns>Lista de proveedores con información de productos y ventas</returns>
    /// <response code="200">Proveedores obtenidos exitosamente</response>
    /// <response code="401">No autorizado - Token JWT requerido</response>
    /// <response code="403">Prohibido - Requiere rol Admin</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SupplierDto>>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllSuppliers()
    {
        try
        {
            _logger.LogInformation("Admin solicitando todos los proveedores");

            var result = await _supplierService.GetAllSuppliersAsync();

            if (result.Success)
            {
                return Ok(result);
            }

            return StatusCode(500, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetAllSuppliers");
            return StatusCode(500, ApiResponse<IEnumerable<SupplierDto>>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Obtiene proveedores con paginación y búsqueda
    /// </summary>
    /// <param name="pageNumber">Número de página (default: 1)</param>
    /// <param name="pageSize">Tamaño de página (default: 20, max: 100)</param>
    /// <param name="searchTerm">Término de búsqueda opcional (nombre, contacto, email, taxId)</param>
    /// <param name="sortBy">Campo para ordenar (default: Name)</param>
    /// <param name="sortDescending">Orden descendente (default: false)</param>
    /// <returns>Proveedores paginados</returns>
    /// <response code="200">Proveedores paginados obtenidos exitosamente</response>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResponse<SupplierDto>), 200)]
    public async Task<IActionResult> GetSuppliersPagedAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "Name",
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            _logger.LogInformation("Admin solicitando proveedores paginados - Página: {PageNumber}, Búsqueda: {SearchTerm}",
                pageNumber, searchTerm ?? "None");

            var result = await _supplierService.GetSuppliersPagedAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetSuppliersPagedAsync");
            return StatusCode(500, PagedResponse<SupplierDto>.Create(
                Enumerable.Empty<SupplierDto>(), pageNumber, pageSize, 0
            ));
        }
    }

    /// <summary>
    /// Obtiene un proveedor específico por su ID
    /// </summary>
    /// <param name="id">ID del proveedor</param>
    /// <returns>Proveedor con estadísticas</returns>
    /// <response code="200">Proveedor encontrado</response>
    /// <response code="404">Proveedor no encontrado</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), 404)]
    public async Task<IActionResult> GetSupplierById(int id)
    {
        try
        {
            _logger.LogInformation("Admin solicitando proveedor Id: {Id}", id);

            var result = await _supplierService.GetSupplierByIdAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetSupplierById: {Id}", id);
            return StatusCode(500, ApiResponse<SupplierDto>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Crea un nuevo proveedor
    /// </summary>
    /// <param name="request">Datos del nuevo proveedor</param>
    /// <returns>Proveedor creado</returns>
    /// <response code="201">Proveedor creado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="409">Ya existe un proveedor con ese nombre, email o TaxId</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), 409)]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<SupplierDto>.ErrorResult(
                    "Datos de entrada inválidos", errors
                ));
            }

            _logger.LogInformation("Admin creando nuevo proveedor: {Name}", request.Name);

            var result = await _supplierService.CreateSupplierAsync(request);

            if (result.Success)
            {
                return CreatedAtAction(
                    nameof(GetSupplierById),
                    new { id = result.Data!.Id },
                    result
                );
            }

            // Si el error es de duplicado, retornar 409 Conflict
            if (result.Message.Contains("existe"))
            {
                return Conflict(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CreateSupplier: {Name}", request.Name);
            return StatusCode(500, ApiResponse<SupplierDto>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Actualiza un proveedor existente
    /// </summary>
    /// <param name="id">ID del proveedor a actualizar</param>
    /// <param name="request">Nuevos datos del proveedor</param>
    /// <returns>Proveedor actualizado</returns>
    /// <response code="200">Proveedor actualizado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Proveedor no encontrado</response>
    /// <response code="409">Ya existe otro proveedor con ese nombre, email o TaxId</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), 404)]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), 409)]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<SupplierDto>.ErrorResult(
                    "Datos de entrada inválidos", errors
                ));
            }

            _logger.LogInformation("Admin actualizando proveedor Id: {Id}", id);

            var result = await _supplierService.UpdateSupplierAsync(id, request);

            if (result.Success)
            {
                return Ok(result);
            }

            // Determinar el tipo de error
            if (result.Message.Contains("no encontrado"))
            {
                return NotFound(result);
            }

            if (result.Message.Contains("existe"))
            {
                return Conflict(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en UpdateSupplier Id: {Id}", id);
            return StatusCode(500, ApiResponse<SupplierDto>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Elimina un proveedor (soft delete)
    /// </summary>
    /// <param name="id">ID del proveedor a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    /// <response code="200">Proveedor eliminado exitosamente</response>
    /// <response code="400">No se puede eliminar - tiene productos asociados</response>
    /// <response code="404">Proveedor no encontrado</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        try
        {
            _logger.LogInformation("Admin eliminando proveedor Id: {Id}", id);

            var result = await _supplierService.DeleteSupplierAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            // Determinar el tipo de error
            if (result.Message.Contains("no encontrado"))
            {
                return NotFound(result);
            }

            if (result.Message.Contains("productos asociados"))
            {
                return BadRequest(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en DeleteSupplier Id: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Obtiene proveedores que suministran un producto específico
    /// </summary>
    /// <param name="productId">ID del producto</param>
    /// <returns>Lista de proveedores que suministran el producto</returns>
    /// <response code="200">Proveedores obtenidos exitosamente</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpGet("by-product/{productId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SupplierDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SupplierDto>>), 404)]
    public async Task<IActionResult> GetSuppliersByProduct(int productId)
    {
        try
        {
            _logger.LogInformation("Admin solicitando proveedores para producto: {ProductId}", productId);

            var result = await _supplierService.GetSuppliersByProductAsync(productId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetSuppliersByProduct: {ProductId}", productId);
            return StatusCode(500, ApiResponse<IEnumerable<SupplierDto>>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }
}