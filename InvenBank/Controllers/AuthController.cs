using Dapper;
using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Entities;
using InvenBank.API.Services;
using InvenBank.API.Services.Implementations;
using InvenBank.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace InvenBank.API.Controllers;

/// <summary>
/// Controlador de autenticación para obtener tokens JWT
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IDbConnection _connection;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IDbConnection connection,
        IJwtService jwtService,
        IPasswordHashService passwordHashService,
        ILogger<AuthController> logger)
    {
        _connection = connection;
        _jwtService = jwtService;
        _passwordHashService = passwordHashService;
        _logger = logger;
    }

    /// <summary>
    /// Autenticación de usuario y generación de token JWT
    /// </summary>
    /// <param name="request">Credenciales de usuario</param>
    /// <returns>Token JWT y datos del usuario</returns>
    /// <response code="200">Login exitoso</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="401">Credenciales incorrectas</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 400)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<LoginResponse>.ErrorResult(
                    "Datos de entrada inválidos", errors
                ));
            }

            _logger.LogInformation("Intento de login para usuario: {Email}", request.Email);

            // Buscar usuario en la base de datos
            var sql = @"
                SELECT u.*, r.Name as RoleName 
                FROM Users u 
                INNER JOIN Roles r ON u.RoleId = r.Id 
                WHERE u.Email = @Email AND u.IsActive = 1";

            var userResult = await _connection.QueryFirstOrDefaultAsync(sql, new { Email = request.Email });

            if (userResult == null)
            {
                _logger.LogWarning("Usuario no encontrado: {Email}", request.Email);
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResult("Credenciales incorrectas"));
            }

            // Crear objeto User desde el resultado de la consulta
            var user = new User
            {
                Id = userResult.Id,
                Email = userResult.Email,
                PasswordHash = userResult.PasswordHash,
                FirstName = userResult.FirstName,
                LastName = userResult.LastName,
                RoleId = userResult.RoleId,
                IsActive = userResult.IsActive,
                LastLoginAt = userResult.LastLoginAt,
                CreatedAt = userResult.CreatedAt,
                UpdatedAt = userResult.UpdatedAt,
                Role = new Role { Id = userResult.RoleId, Name = userResult.RoleName }
            };

            // Verificar contraseña
            var isValidPassword = _passwordHashService.VerifyPassword(request.Password, user.PasswordHash);

            if (!isValidPassword)
            {
                _logger.LogWarning("Contraseña incorrecta para usuario: {Email}", request.Email);
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResult("Credenciales incorrectas"));
            }

            // Generar token JWT
            var loginResponse = await _jwtService.GenerateTokenAsync(user);

            // Actualizar último login
            await _connection.ExecuteAsync(
                "UPDATE Users SET LastLoginAt = GETDATE() WHERE Id = @Id",
                new { Id = user.Id }
            );

            _logger.LogInformation("Login exitoso para usuario: {Email} - Role: {Role}", request.Email, user.Role.Name);

            return Ok(ApiResponse<LoginResponse>.SuccessResult(
                loginResponse,
                "Login exitoso"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en login para usuario: {Email}", request.Email);
            return StatusCode(500, ApiResponse<LoginResponse>.ErrorResult(
                "Error interno del servidor"
            ));
        }
    }

    /// <summary>
    /// Endpoint temporal para crear usuario admin (solo para testing)
    /// </summary>
    /// <returns>Usuario admin creado</returns>
    [HttpPost("create-admin-temp")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> CreateTempAdmin()
    {
        try
        {
            // Verificar si ya existe admin
            var existingAdmin = await _connection.QueryFirstOrDefaultAsync(
                "SELECT Id FROM Users WHERE Email = @Email",
                new { Email = "admin2@invenbank.com" }
            );

            if (existingAdmin != null)
            {
                return Ok(ApiResponse<object>.SuccessResult(
                    new { Message = "Admin ya existe", Email = "admin2@invenbank.com" },
                    "Usuario admin ya está disponible"
                ));
            }

            // Crear usuario admin temporal
            var adminRoleId = await _connection.QueryFirstOrDefaultAsync<int>(
                "SELECT Id FROM Roles WHERE Name = 'Admin'"
            );

            if (adminRoleId == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Rol Admin no encontrado en la base de datos"));
            }

            var passwordHash = _passwordHashService.HashPassword("Admin123*");

            var sql = @"
                INSERT INTO Users (Email, PasswordHash, FirstName, LastName, RoleId, IsActive, CreatedAt, UpdatedAt)
                VALUES (@Email, @PasswordHash, @FirstName, @LastName, @RoleId, 1, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            var userId = await _connection.QuerySingleAsync<int>(sql, new
            {
                Email = "admin2@invenbank.com",
                PasswordHash = passwordHash,
                FirstName = "Admin2",
                LastName = "InvenBank",
                RoleId = adminRoleId
            });

            _logger.LogInformation("Usuario admin temporal creado con Id: {UserId}", userId);

            return Ok(ApiResponse<object>.SuccessResult(
                new
                {
                    UserId = userId,
                    Email = "admin2@invenbank.com",
                    Password = "Admin123*",
                    Message = "Usuario admin creado exitosamente"
                },
                "Usuario admin temporal creado"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario admin temporal");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Error al crear usuario admin"));
        }
    }

    /// <summary>
    /// Obtiene información del usuario autenticado
    /// </summary>
    /// <returns>Datos del usuario actual</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<UserDto>.ErrorResult("Token inválido"));
            }

            var sql = @"
                SELECT u.*, r.Name as RoleName 
                FROM Users u 
                INNER JOIN Roles r ON u.RoleId = r.Id 
                WHERE u.Id = @UserId AND u.IsActive = 1";

            var userResult = await _connection.QueryFirstOrDefaultAsync(sql, new { UserId = userId });

            if (userResult == null)
            {
                return Unauthorized(ApiResponse<UserDto>.ErrorResult("Usuario no encontrado"));
            }

            var userDto = new UserDto
            {
                Id = userResult.Id,
                Email = userResult.Email,
                FirstName = userResult.FirstName,
                LastName = userResult.LastName,
                FullName = $"{userResult.FirstName} {userResult.LastName}",
                RoleId = userResult.RoleId,
                RoleName = userResult.RoleName,
                IsActive = userResult.IsActive,
                LastLoginAt = userResult.LastLoginAt,
                CreatedAt = userResult.CreatedAt,
                UpdatedAt = userResult.UpdatedAt
            };

            return Ok(ApiResponse<UserDto>.SuccessResult(userDto, "Usuario obtenido exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario actual");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResult("Error interno del servidor"));
        }
    }
}