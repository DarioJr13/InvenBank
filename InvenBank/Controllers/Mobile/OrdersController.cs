using Dapper;
using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text.Json;

namespace InvenBank.API.Controllers.Client;

[ApiController]
[Route("api/mobile/[controller]")]
[Authorize(Roles = "Customer")]
public class OrdersController : ControllerBase
{
    private readonly IDbConnection _connection;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IDbConnection connection, ILogger<OrdersController> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    /// <summary>
    /// Crear orden
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] object request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            // Asegurar que la conexión esté abierta
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            using var transaction = _connection.BeginTransaction();

            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(request);
                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                var shippingAddress = data.ContainsKey("shippingAddress") ? data["shippingAddress"].GetString() : "";
                var paymentMethod = data.ContainsKey("paymentMethod") ? data["paymentMethod"].GetString() : "";

                if (!data.ContainsKey("items"))
                    return BadRequest(ApiResponse<object>.ErrorResult("Items requeridos"));

                var itemsJson = System.Text.Json.JsonSerializer.Serialize(data["items"]);
                var items = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(itemsJson);

                decimal totalAmount = 0;

                // Validar y calcular total
                foreach (var item in items)
                {
                    var productId = item["productId"].GetInt32();
                    var supplierId = item["supplierId"].GetInt32();
                    var quantity = item["quantity"].GetInt32();

                    var productSql = @"
                        SELECT ps.Price, ps.Stock
                        FROM ProductSuppliers ps
                        WHERE ps.ProductId = @ProductId AND ps.SupplierId = @SupplierId 
                        AND ps.IsActive = 1";

                    var productInfo = await _connection.QuerySingleOrDefaultAsync<(decimal Price, int Stock)>(
                        productSql, new { ProductId = productId, SupplierId = supplierId }, transaction);

                    if (productInfo.Price == 0)
                        return BadRequest(ApiResponse<object>.ErrorResult($"Producto {productId} no disponible"));

                    if (productInfo.Stock < quantity)
                        return BadRequest(ApiResponse<object>.ErrorResult($"Stock insuficiente"));

                    totalAmount += productInfo.Price * quantity;
                }

                // Generar OrderNumber automático único
                var today = DateTime.Now.ToString("yyyyMMdd");
                var lastNumber = await _connection.QuerySingleOrDefaultAsync<int?>(@"
                    SELECT MAX(CAST(RIGHT(OrderNumber, 4) AS INT))
                    FROM Orders 
                    WHERE OrderNumber LIKE @Pattern",
                    new { Pattern = $"ORD-{today}-%" }, transaction);

                var nextNumber = (lastNumber ?? 0) + 1;
                var orderNumber = $"ORD-{today}-{nextNumber:D4}";

                // Crear orden
                var orderSql = @"
                    INSERT INTO Orders (OrderNumber, UserId, OrderDate, Status, TotalAmount, ShippingAddress)
                    OUTPUT INSERTED.Id
                    VALUES (@OrderNumber, @UserId, @OrderDate, @Status, @TotalAmount, @ShippingAddress)";

                var orderId = await _connection.QuerySingleAsync<int>(orderSql, new
                {
                    OrderNumber = orderNumber,
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    TotalAmount = totalAmount,
                    ShippingAddress = shippingAddress
                }, transaction);

                // Crear detalles y actualizar stock
                foreach (var item in items)
                {
                    var productId = item["productId"].GetInt32();
                    var supplierId = item["supplierId"].GetInt32();
                    var quantity = item["quantity"].GetInt32();

                    // Obtener ProductSupplierId y precio
                    var productSupplierInfo = await _connection.QuerySingleAsync<(int Id, decimal Price)>(@"
                        SELECT Id, Price 
                        FROM ProductSuppliers 
                        WHERE ProductId = @ProductId AND SupplierId = @SupplierId AND IsActive = 1",
                        new { ProductId = productId, SupplierId = supplierId }, transaction);

                    // Insertar detalle con estructura correcta (sin TotalPrice - es columna calculada)
                    await _connection.ExecuteAsync(@"
                        INSERT INTO OrderDetails (OrderId, ProductSupplierId, Quantity, UnitPrice, CreatedAt)
                        VALUES (@OrderId, @ProductSupplierId, @Quantity, @UnitPrice, @CreatedAt)", new
                    {
                        OrderId = orderId,
                        ProductSupplierId = productSupplierInfo.Id,
                        Quantity = quantity,
                        UnitPrice = productSupplierInfo.Price,
                        CreatedAt = DateTime.UtcNow
                    }, transaction);

                    // Actualizar stock
                    await _connection.ExecuteAsync(@"
                        UPDATE ProductSuppliers 
                        SET Stock = Stock - @Quantity
                        WHERE Id = @ProductSupplierId", new
                    {
                        Quantity = quantity,
                        ProductSupplierId = productSupplierInfo.Id
                    }, transaction);
                }

                transaction.Commit();

                return Ok(ApiResponse<object>.SuccessResult(new { orderId, totalAmount }, "Orden creada"));
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando orden");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error interno"));
        }
    }


    /// <summary>
    /// Obtener órdenes del usuario
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var offset = (page - 1) * pageSize;

            var sql = @"
                SELECT 
                    o.Id,
                    o.OrderDate,
                    o.Status,
                    o.TotalAmount,
                    COUNT(od.Id) AS ItemCount
                FROM Orders o
                LEFT JOIN OrderDetails od ON o.Id = od.OrderId
                WHERE o.UserId = @UserId
                GROUP BY o.Id, o.OrderDate, o.Status, o.TotalAmount
                ORDER BY o.OrderDate DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var orders = await _connection.QueryAsync(sql, new
            {
                UserId = userId,
                Offset = offset,
                PageSize = pageSize
            });

            return Ok(ApiResponse<object>.SuccessResult(orders, "Órdenes obtenidas"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo órdenes");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error interno"));
        }
    }

    /// <summary>
    /// Detalle de orden
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var orderSql = @"
                SELECT o.Id, o.OrderDate, o.Status, o.TotalAmount, o.ShippingAddress, o.PaymentMethod
                FROM Orders o
                WHERE o.Id = @Id AND o.UserId = @UserId";

            var order = await _connection.QuerySingleOrDefaultAsync(orderSql, new { Id = id, UserId = userId });

            if (order == null)
                return NotFound(ApiResponse<object>.ErrorResult("Orden no encontrada"));

            var detailsSql = @"
                SELECT 
                    od.ProductId,
                    p.Name AS ProductName,
                    s.Name AS SupplierName,
                    od.Quantity,
                    od.UnitPrice,
                    od.Subtotal
                FROM OrderDetails od
                INNER JOIN Products p ON od.ProductId = p.Id
                INNER JOIN Suppliers s ON od.SupplierId = s.Id
                WHERE od.OrderId = @Id";

            var details = await _connection.QueryAsync(detailsSql, new { Id = id });

            return Ok(ApiResponse<object>.SuccessResult(new { order, details }, "Detalle obtenido"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo detalle de orden");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error interno"));
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}