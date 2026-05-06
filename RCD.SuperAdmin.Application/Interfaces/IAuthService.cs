using RCD.SuperAdmin.Application.DTOs.Auth;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, string? ipAddress);
    Task<RefreshTokenResponse?> RefreshAsync(string refreshToken, string? ipAddress);
    Task RevocarRefreshTokenAsync(string refreshToken);
    Task<LoginResponse?> LoginConQrAsync(string qrCode, string? ipAddress = null, string? plataforma = null, CancellationToken ct = default);
    Task<IEnumerable<ProyectoPermitidoDto>> DescubrirProyectosAsync(string identificador, CancellationToken ct = default);
}
