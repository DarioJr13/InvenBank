using Dapper;
using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace InvenBank.API.Controllers.Mobile;

[ApiController]
[Route("api/mobile/[controller]")]
[Authorize(Roles = "Customer")]
public class CatalogController : ControllerBase
{
    private readonly IDbConnection _connection;
    private readonly ILogger<CatalogController> _logger;

    public CatalogController(IDbConnection connection, ILogger<CatalogController> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    /// <summary>
    /// Búsqueda de productos
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var offset = (page - 1) * pageSize;
            var whereConditions = new List<string> { "p.IsActive = 1", "ps.IsActive = 1" };
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(search))
            {
                whereConditions.Add("(p.Name LIKE @Search OR p.Description LIKE @Search)");
                parameters.Add("Search", $"%{search}%");
            }

            if (categoryId.HasValue)
            {
                whereConditions.Add("p.CategoryId = @CategoryId");
                parameters.Add("CategoryId", categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                whereConditions.Add("ps.Price >= @MinPrice");
                parameters.Add("MinPrice", minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                whereConditions.Add("ps.Price <= @MaxPrice");
                parameters.Add("MaxPrice", maxPrice.Value);
            }

            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);

            var whereClause = string.Join(" AND ", whereConditions);

            var sql = $@"
                SELECT 
                    p.Id,
                    p.Name,
                    CAST(p.Description AS NVARCHAR(MAX)) AS Description,
                    CAST(p.ImageUrl AS NVARCHAR(500)) AS ImageUrl,
                    c.Name AS Category,
                    MIN(ps.Price) AS MinPrice,
                    MAX(ps.Price) AS MaxPrice,
                    SUM(ps.Stock) AS TotalStock
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                INNER JOIN ProductSuppliers ps ON p.Id = ps.ProductId
                WHERE {whereClause}
                GROUP BY p.Id, p.Name, CAST(p.Description AS NVARCHAR(MAX)), CAST(p.ImageUrl AS NVARCHAR(500)), c.Name
                ORDER BY MIN(ps.Price) ASC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var products = await _connection.QueryAsync(sql, parameters);

            return Ok(ApiResponse<object>.SuccessResult(products, "Productos encontrados"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en búsqueda");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error interno"));
        }
    }

    /// <summary>
    /// Detalle de producto
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductDetail(int id)
    {
        try
        {
            var sql = @"
                SELECT 
                    p.Id,
                    p.Name,
                    p.Description,
                    p.ImageUrl,
                    p.Specifications,
                    c.Name AS Category
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.Id = @Id AND p.IsActive = 1";

            var product = await _connection.QueryFirstOrDefaultAsync(sql, new { Id = id });

            if (product == null)
                return NotFound(ApiResponse<object>.ErrorResult("Producto no encontrado"));

            // Obtener precios por proveedor
            var suppliersSql = @"
                SELECT 
                    s.Id,
                    s.Name,
                    ps.Price,
                    ps.Stock
                FROM ProductSuppliers ps
                INNER JOIN Suppliers s ON ps.SupplierId = s.Id
                WHERE ps.ProductId = @Id AND ps.IsActive = 1
                ORDER BY ps.Price ASC";

            var suppliers = await _connection.QueryAsync(suppliersSql, new { Id = id });

            return Ok(ApiResponse<object>.SuccessResult(new { product, suppliers }, "Detalle obtenido"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo detalle");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error interno"));
        }
    }

    /// <summary>
    /// Categorías disponibles
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var sql = @"
                SELECT 
                    c.Id,
                    c.Name,
                    c.Description,
                    COUNT(DISTINCT p.Id) AS ProductCount
                FROM Categories c
                LEFT JOIN Products p ON c.Id = p.CategoryId AND p.IsActive = 1
                WHERE c.IsActive = 1
                GROUP BY c.Id, c.Name, c.Description
                ORDER BY c.Name";

            var categories = await _connection.QueryAsync(sql);

            return Ok(ApiResponse<object>.SuccessResult(categories, "Categorías obtenidas"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo categorías");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error interno"));
        }
    }
}