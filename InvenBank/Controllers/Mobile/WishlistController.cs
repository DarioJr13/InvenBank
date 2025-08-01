using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data;
using System.Text.Json;

namespace InvenBank.API.Controllers.Mobile;

[ApiController]
[Route("api/mobile/[controller]")]
[Authorize(Roles = "Customer")]
public class WishlistController : ControllerBase
{
    private readonly IDbConnection _connection;
    private readonly ILogger<WishlistController> _logger;

    public WishlistController(IDbConnection connection, ILogger<WishlistController> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    /// <summary>
    /// Obtener wishlist
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var sql = @"
                SELECT 
                    w.Id AS WishlistId,
                    p.Id AS ProductId,
                    p.Name AS ProductName,
                    cast(p.Description as varchar(max)) as Description,
                    cast(p.ImageUrl as varchar(max)) as ImageUrl,
                    c.Name AS Category,
                    MIN(ps.Price) AS MinPrice,
                    SUM(ps.Stock) AS TotalStock,
                    w.AddedDate
                FROM Wishlists w
                INNER JOIN Products p ON w.ProductId = p.Id
                INNER JOIN Categories c ON p.CategoryId = c.Id
                INNER JOIN ProductSuppliers ps ON p.Id = ps.ProductId
                WHERE w.UserId = @UserId AND p.IsActive = 1
                GROUP BY w.Id, p.Id, p.Name, cast(p.Description as varchar(max)), cast(p.ImageUrl as varchar(max)), c.Name, w.AddedDate
                ORDER BY w.AddedDate DESC";

            var wishlist = await _connection.QueryAsync(sql, new { UserId = userId });

            return Ok(ApiResponse<object>.SuccessResult(wishlist, "Wishlist obtenida"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo wishlist");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error interno"));
        }
    }

    /// <summary>
    /// Agregar a wishlist
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddToWishlist([FromBody] object request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            // Parsear productId del request
            var json = System.Text.Json.JsonSerializer.Serialize(request);
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            if (!data.ContainsKey("productId"))
                return BadRequest(ApiResponse<object>.ErrorResult("ProductId requerido"));

            var productId = data["productId"].GetInt32();

            // Verificar si ya existe
            var existsSql = "SELECT COUNT(1) FROM Wishlists WHERE UserId = @UserId AND ProductId = @ProductId";
            var exists = await _connection.QuerySingleAsync<int>(existsSql,
                new { UserId = userId, ProductId = productId });

            if (exists > 0)
                return Conflict(ApiResponse<object>.ErrorResult("Ya está en wishlist"));

            // Agregar
            var insertSql = @"
                INSERT INTO Wishlists (UserId, ProductId, AddedDate)
                VALUES (@UserId, @ProductId, @CreatedAt)";

            await _connection.ExecuteAsync(insertSql, new
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            });

            return Ok(ApiResponse<object>.SuccessResult(null, "Agregado a wishlist"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error agregando a wishlist");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error interno"));
        }
    }

    /// <summary>
    /// Remover de wishlist
    /// </summary>
    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveFromWishlist(int productId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var deleteSql = "DELETE FROM Wishlists WHERE UserId = @UserId AND ProductId = @ProductId";
            var deleted = await _connection.ExecuteAsync(deleteSql,
                new { UserId = userId, ProductId = productId });

            if (deleted == 0)
                return NotFound(ApiResponse<object>.ErrorResult("No encontrado en wishlist"));

            return Ok(ApiResponse<object>.SuccessResult(null, "Removido de wishlist"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removiendo de wishlist");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error interno"));
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}