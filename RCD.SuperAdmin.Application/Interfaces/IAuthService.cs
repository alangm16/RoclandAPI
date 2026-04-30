using RCD.SuperAdmin.Application.DTOs.Auth;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, string? ipAddress);
    Task<RefreshTokenResponse?> RefreshAsync(string refreshToken, string? ipAddress);
    Task RevocarRefreshTokenAsync(string refreshToken);
}
