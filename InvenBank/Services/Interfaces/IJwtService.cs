using InvenBank.API.DTOs.Responses;
using InvenBank.API.Entities;
using System.Security.Claims;

namespace InvenBank.API.Services.Interfaces
{
    public interface IJwtService
    {
        Task<LoginResponse> GenerateTokenAsync(User user);
        Task<bool> ValidateTokenAsync(string token);
        Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token);
        Task<LoginResponse?> RefreshTokenAsync(string token, string refreshToken);
        string GenerateRefreshToken();
    }
}
