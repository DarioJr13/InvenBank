using Dapper;
using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace InvenBank.API.Controllers;

/// <summary>
/// Controlador para verificar el estado de salud de la API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IDbConnection _connection;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IDbConnection connection, ILogger<HealthController> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    /// <summary>
    /// Verifica el estado básico de la API
    /// </summary>
    /// <returns>Estado de la API</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Get()
    {
        try
        {
            var response = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                MachineName = Environment.MachineName,
                ProcessId = Environment.ProcessId
            };

            _logger.LogInformation("Health check ejecutado exitosamente");

            return Ok(ApiResponse<object>.SuccessResult(response, "API funcionando correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en health check");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Verifica la conexión a la base de datos
    /// </summary>
    /// <returns>Estado de la conexión a BD</returns>
    [HttpGet("database")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> CheckDatabase()
    {
        try
        {
            var startTime = DateTime.UtcNow;

            // Ejecutar query simple para verificar conexión
            var result = await _connection.QuerySingleAsync<int>("SELECT 1");

            var endTime = DateTime.UtcNow;
            var responseTime = (endTime - startTime).TotalMilliseconds;

            var response = new
            {
                DatabaseStatus = "Connected",
                ResponseTimeMs = responseTime,
                Timestamp = DateTime.UtcNow,
                QueryResult = result
            };

            _logger.LogInformation("Database health check ejecutado exitosamente en {ResponseTime}ms", responseTime);

            return Ok(ApiResponse<object>.SuccessResult(response, "Conexión a base de datos exitosa"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en database health check");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error de conexión a base de datos"));
        }
    }

    /// <summary>
    /// Verifica las tablas principales de InvenBank
    /// </summary>
    /// <returns>Conteo de registros en tablas principales</returns>
    [HttpGet("tables")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> CheckTables()
    {
        try
        {
            var queries = new Dictionary<string, string>
            {
                { "Users", "SELECT COUNT(*) FROM Users WHERE IsActive = 1" },
                { "Products", "SELECT COUNT(*) FROM Products WHERE IsActive = 1" },
                { "Categories", "SELECT COUNT(*) FROM Categories WHERE IsActive = 1" },
                { "Suppliers", "SELECT COUNT(*) FROM Suppliers WHERE IsActive = 1" },
                { "ProductSuppliers", "SELECT COUNT(*) FROM ProductSuppliers WHERE IsActive = 1" },
                { "Orders", "SELECT COUNT(*) FROM Orders" },
                { "Wishlists", "SELECT COUNT(*) FROM Wishlists" }
            };

            var tableCounts = new Dictionary<string, int>();

            foreach (var query in queries)
            {
                try
                {
                    var count = await _connection.QuerySingleAsync<int>(query.Value);
                    tableCounts[query.Key] = count;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al consultar tabla {TableName}", query.Key);
                    tableCounts[query.Key] = -1; // Indica error
                }
            }

            var response = new
            {
                TableCounts = tableCounts,
                Timestamp = DateTime.UtcNow,
                TotalTables = queries.Count,
                SuccessfulQueries = tableCounts.Count(x => x.Value >= 0)
            };

            _logger.LogInformation("Tables health check ejecutado exitosamente");

            return Ok(ApiResponse<object>.SuccessResult(response, "Verificación de tablas completada"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en tables health check");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error al verificar tablas"));
        }
    }

    /// <summary>
    /// Verifica la configuración de JWT (requiere autenticación)
    /// </summary>
    /// <returns>Información del token JWT</returns>
    [HttpGet("auth")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CheckAuth()
    {
        try
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            var response = new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                UserId = User.FindFirst("userId")?.Value,
                Email = User.FindFirst("email")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/soap/envelope/")?.Value,
                Role = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value,
                Claims = claims,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Auth health check ejecutado exitosamente para usuario: {UserId}", response.UserId);

            return Ok(ApiResponse<object>.SuccessResult(response, "Autenticación verificada exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en auth health check");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error al verificar autenticación"));
        }
    }

    /// <summary>
    /// Obtiene información detallada del sistema
    /// </summary>
    /// <returns>Información del sistema</returns>
    [HttpGet("system")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> GetSystemInfo()
    {
        try
        {
            var response = new
            {
                Server = new
                {
                    MachineName = Environment.MachineName,
                    ProcessorCount = Environment.ProcessorCount,
                    OSVersion = Environment.OSVersion.ToString(),
                    WorkingSet = Environment.WorkingSet,
                    ProcessId = Environment.ProcessId,
                    Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                    Is64BitProcess = Environment.Is64BitProcess
                },
                Application = new
                {
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                    FrameworkVersion = Environment.Version.ToString(),
                    StartTime = DateTime.UtcNow // Podrías almacenar el tiempo real de inicio
                },
                Memory = new
                {
                    WorkingSetMB = Math.Round(Environment.WorkingSet / 1024.0 / 1024.0, 2),
                    GCTotalMemoryMB = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2)
                }
            };

            _logger.LogInformation("System info obtenida exitosamente");

            return Ok(ApiResponse<object>.SuccessResult(response, "Información del sistema obtenida"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener system info");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error al obtener información del sistema"));
        }
    }
}