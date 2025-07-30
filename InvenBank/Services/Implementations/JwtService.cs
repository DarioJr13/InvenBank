using InvenBank.API.Configuration;
using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Entities;
using InvenBank.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InvenBank.API.Services.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtService> _logger;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;

            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero // Remove delay of token expiry
            };
        }

        // ===============================================
        // GENERAR TOKEN Y REFRESH TOKEN
        // ===============================================

        public async Task<LoginResponse> GenerateTokenAsync(User user)
        {
            try
            {
                _logger.LogInformation("Generando token para usuario: {Email}", user.Email);

                var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Role, user.Role.Name),
                new("userId", user.Id.ToString()),
                new("roleId", user.RoleId.ToString()),
                new("firstName", user.FirstName),
                new("lastName", user.LastName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

                var expiryTime = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

                var jwtSecurityToken = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: expiryTime,
                    signingCredentials: signingCredentials
                );

                var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                var refreshToken = GenerateRefreshToken();

                _logger.LogInformation("Token generado exitosamente para usuario: {Email}", user.Email);

                return new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiryTime,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
                        RoleId = user.RoleId,
                        RoleName = user.Role.Name,
                        IsActive = user.IsActive,
                        LastLoginAt = user.LastLoginAt,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar token para usuario: {Email}", user.Email);
                throw;
            }
        }

        // ===============================================
        // VALIDAR TOKEN
        // ===============================================

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                await tokenHandler.ValidateTokenAsync(token, _tokenValidationParameters);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token inválido: {Token}", token);
                return false;
            }
        }

        // ===============================================
        // OBTENER PRINCIPAL DEL TOKEN
        // ===============================================

        public async Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                var validationResult = await tokenHandler.ValidateTokenAsync(token, _tokenValidationParameters);

                if (validationResult.IsValid)
                {
                    return new ClaimsPrincipal(validationResult.ClaimsIdentity);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener principal del token: {Token}", token);
                return null;
            }
        }

        // ===============================================
        // REFRESH TOKEN
        // ===============================================

        public async Task<LoginResponse?> RefreshTokenAsync(string token, string refreshToken)
        {
            try
            {
                _logger.LogInformation("Intentando refrescar token");

                var principal = await GetPrincipalFromTokenAsync(token);

                if (principal == null)
                {
                    _logger.LogWarning("Principal no válido para refresh token");
                    return null;
                }

                // Aquí deberías validar el refresh token contra la base de datos
                // Por simplicidad, asumimos que es válido

                var userIdClaim = principal.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("UserId no encontrado en token para refresh");
                    return null;
                }

                // Aquí deberías obtener el usuario actual de la base de datos
                // Por ahora retornamos null indicando que necesita implementación adicional

                _logger.LogInformation("Refresh token procesado para userId: {UserId}", userId);

                return null; // Implementar lógica completa con base de datos
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar refresh token");
                return null;
            }
        }

        // ===============================================
        // GENERAR REFRESH TOKEN
        // ===============================================

        public string GenerateRefreshToken()
        {
            try
            {
                var randomNumber = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar refresh token");
                throw;
            }
        }
    }

    // ===============================================
    // SERVICIO DE HASH DE CONTRASEÑAS
    // ===============================================

    public interface IPasswordHashService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public class PasswordHashService : IPasswordHashService
    {
        private readonly ILogger<PasswordHashService> _logger;

        public PasswordHashService(ILogger<PasswordHashService> logger)
        {
            _logger = logger;
        }

        public string HashPassword(string password)
        {
            try
            {
                // Usando BCrypt para hash de contraseñas
                return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al hashear contraseña");
                throw;
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar contraseña");
                return false;
            }
        }
    }
}
