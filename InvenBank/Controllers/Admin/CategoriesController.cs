using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Repositories.Interfaces;
using InvenBank.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenBank.API.Controllers.Admin;

/// <summary>
/// Controlador para la gestión administrativa de categorías
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService categoryService,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las categorías con estadísticas
    /// </summary>
    /// <returns>Lista de categorías con información de productos y ventas</returns>
    /// <response code="200">Categorías obtenidas exitosamente</response>
    /// <response code="401">No autorizado - Token JWT requerido</response>
    /// <response code="403">Prohibido - Requiere rol Admin</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllCategories()
    {
        try
        {
            _logger.LogInformation("Admin solicitando todas las categorías");

            var result = await _categoryService.GetAllCategoriesAsync();

            if (result.Success)
            {
                return Ok(result);
            }

            return StatusCode(500, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetAllCategories");
            return StatusCode(500, ApiResponse<IEnumerable<CategoryDto>>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Obtiene categorías con paginación y búsqueda
    /// </summary>
    /// <param name="pageNumber">Número de página (default: 1)</param>
    /// <param name="pageSize">Tamaño de página (default: 20, max: 100)</param>
    /// <param name="searchTerm">Término de búsqueda opcional</param>
    /// <param name="sortBy">Campo para ordenar (default: Name)</param>
    /// <param name="sortDescending">Orden descendente (default: false)</param>
    /// <returns>Categorías paginadas</returns>
    /// <response code="200">Categorías paginadas obtenidas exitosamente</response>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResponse<CategoryDto>), 200)]
    public async Task<IActionResult> GetCategoriesPaged(
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

            _logger.LogInformation("Admin solicitando categorías paginadas - Página: {PageNumber}, Búsqueda: {SearchTerm}",
                pageNumber, searchTerm ?? "None");

            var result = await _categoryService.GetCategoriesPagedAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetCategoriesPaged");
            return StatusCode(500, PagedResponse<CategoryDto>.Create(
                Enumerable.Empty<CategoryDto>(), pageNumber, pageSize, 0
            ));
        }
    }

    /// <summary>
    /// Obtiene una categoría específica por su ID
    /// </summary>
    /// <param name="id">ID de la categoría</param>
    /// <returns>Categoría con estadísticas</returns>
    /// <response code="200">Categoría encontrada</response>
    /// <response code="404">Categoría no encontrada</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 404)]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        try
        {
            _logger.LogInformation("Admin solicitando categoría Id: {Id}", id);

            var result = await _categoryService.GetCategoryByIdAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetCategoryById: {Id}", id);
            return StatusCode(500, ApiResponse<CategoryDto>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Crea una nueva categoría
    /// </summary>
    /// <param name="request">Datos de la nueva categoría</param>
    /// <returns>Categoría creada</returns>
    /// <response code="201">Categoría creada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="409">Ya existe una categoría con ese nombre</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 409)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<CategoryDto>.ErrorResult(
                    "Datos de entrada inválidos", errors
                ));
            }

            _logger.LogInformation("Admin creando nueva categoría: {Name}", request.Name);

            var result = await _categoryService.CreateCategoryAsync(request);

            if (result.Success)
            {
                return CreatedAtAction(
                    nameof(GetCategoryById),
                    new { id = result.Data!.Id },
                    result
                );
            }

            // Si el error es de nombre duplicado, retornar 409 Conflict
            if (result.Message.Contains("existe"))
            {
                return Conflict(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CreateCategory: {Name}", request.Name);
            return StatusCode(500, ApiResponse<CategoryDto>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Actualiza una categoría existente
    /// </summary>
    /// <param name="id">ID de la categoría a actualizar</param>
    /// <param name="request">Nuevos datos de la categoría</param>
    /// <returns>Categoría actualizada</returns>
    /// <response code="200">Categoría actualizada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Categoría no encontrada</response>
    /// <response code="409">Ya existe otra categoría con ese nombre</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 404)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 409)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<CategoryDto>.ErrorResult(
                    "Datos de entrada inválidos", errors
                ));
            }

            _logger.LogInformation("Admin actualizando categoría Id: {Id}", id);

            var result = await _categoryService.UpdateCategoryAsync(id, request);

            if (result.Success)
            {
                return Ok(result);
            }

            // Determinar el tipo de error
            if (result.Message.Contains("no encontrada"))
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
            _logger.LogError(ex, "Error en UpdateCategory Id: {Id}", id);
            return StatusCode(500, ApiResponse<CategoryDto>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Elimina una categoría (soft delete)
    /// </summary>
    /// <param name="id">ID de la categoría a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    /// <response code="200">Categoría eliminada exitosamente</response>
    /// <response code="400">No se puede eliminar - tiene dependencias</response>
    /// <response code="404">Categoría no encontrada</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            _logger.LogInformation("Admin eliminando categoría Id: {Id}", id);

            var result = await _categoryService.DeleteCategoryAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            // Determinar el tipo de error
            if (result.Message.Contains("no encontrada"))
            {
                return NotFound(result);
            }

            if (result.Message.Contains("subcategorías") || result.Message.Contains("productos"))
            {
                return BadRequest(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en DeleteCategory Id: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Obtiene la jerarquía completa de categorías
    /// </summary>
    /// <returns>Categorías organizadas jerárquicamente</returns>
    /// <response code="200">Jerarquía obtenida exitosamente</response>
    [HttpGet("hierarchy")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), 200)]
    public async Task<IActionResult> GetCategoryHierarchy()
    {
        try
        {
            _logger.LogInformation("Admin solicitando jerarquía de categorías");

            var result = await _categoryService.GetCategoryHierarchyAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetCategoryHierarchy");
            return StatusCode(500, ApiResponse<IEnumerable<CategoryDto>>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Obtiene categorías por categoría padre
    /// </summary>
    /// <param name="parentId">ID de la categoría padre (null para categorías principales)</param>
    /// <returns>Categorías hijas</returns>
    /// <response code="200">Categorías hijas obtenidas exitosamente</response>
    [HttpGet("by-parent")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), 200)]
    public async Task<IActionResult> GetCategoriesByParent([FromQuery] int? parentId = null)
    {
        try
        {
            _logger.LogInformation("Admin solicitando categorías por padre: {ParentId}",
                parentId?.ToString() ?? "NULL");

            var result = await _categoryService.GetCategoriesByParentAsync(parentId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetCategoriesByParent: {ParentId}", parentId);
            return StatusCode(500, ApiResponse<IEnumerable<CategoryDto>>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }
}