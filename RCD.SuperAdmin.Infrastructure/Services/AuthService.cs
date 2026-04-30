// AuthService.cs
using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.Auth;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class AuthService(
    SuperAdminDbContext db,
    ITokenService tokenService,
    IPermisosService permisosService) : IAuthService
{
    private const int MaxIntentosFallidos = 5;
    private const int MinutosBloqueo = 15;

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, string? ipAddress)
    {
        var usuario = await db.Usuarios
            .Include(u => u.Roles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        // --- Registrar intento (exitoso o no) al final, pero evaluar primero ---

        if (usuario is null)
        {
            await RegistrarLogAsync(null, request.Username, false,
                ipAddress, request.Plataforma, "Usuario no encontrado");
            return null;
        }

        // Cuenta bloqueada
        if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta > DateTime.UtcNow)
        {
            await RegistrarLogAsync(usuario.Id, request.Username, false,
                ipAddress, request.Plataforma,
                $"Cuenta bloqueada hasta {usuario.BloqueadoHasta:HH:mm}");
            return null;
        }

        // Contraseña incorrecta
        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= MaxIntentosFallidos)
                usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(MinutosBloqueo);

            await db.SaveChangesAsync();
            await RegistrarLogAsync(usuario.Id, request.Username, false,
                ipAddress, request.Plataforma, "Contraseña incorrecta");
            return null;
        }

        // --- Login exitoso ---
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        usuario.UltimoAcceso = DateTime.UtcNow;

        var roles = usuario.Roles.Select(ur => ur.Rol.Nombre).ToList();
        var accessToken = tokenService.GenerarAccessToken(usuario.Id, usuario.Username, roles);
        var expira = tokenService.ObtenerExpiracionAccessToken();
        var refreshTokenStr = tokenService.GenerarRefreshToken();

        // Guardar refresh token
        db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuario.Id,
            Token = refreshTokenStr,
            FechaExpiracion = DateTime.UtcNow.AddDays(30),
            IpCreacion = ipAddress,
            DispositivoInfo = request.DispositivoInfo
        });

        await db.SaveChangesAsync();

        await RegistrarLogAsync(usuario.Id, request.Username, true,
            ipAddress, request.Plataforma, "Login exitoso");

        var proyectos = await permisosService.ResolverPermisosEfectivosAsync(usuario.Id);

        return new LoginResponse(
            accessToken, refreshTokenStr, expira,
            usuario.NombreCompleto, usuario.Username,
            roles, proyectos);
    }

    public async Task<RefreshTokenResponse?> RefreshAsync(string refreshToken, string? ipAddress)
    {
        var tokenEntity = await db.RefreshTokens
            .Include(rt => rt.Usuario)
            .ThenInclude(u => u.Roles)
            .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity is null || tokenEntity.Revocado ||
            tokenEntity.FechaExpiracion < DateTime.UtcNow ||
            !tokenEntity.Usuario.Activo)
            return null;

        // Rotar: revocar el actual y emitir uno nuevo
        tokenEntity.Revocado = true;

        var usuario = tokenEntity.Usuario;
        var roles = usuario.Roles.Select(ur => ur.Rol.Nombre).ToList();
        var nuevoAccessToken = tokenService.GenerarAccessToken(
            usuario.Id, usuario.Username, roles);
        var nuevoRefreshToken = tokenService.GenerarRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuario.Id,
            Token = nuevoRefreshToken,
            FechaExpiracion = DateTime.UtcNow.AddDays(30),
            IpCreacion = ipAddress
        });

        await db.SaveChangesAsync();

        return new RefreshTokenResponse(
            nuevoAccessToken, nuevoRefreshToken,
            tokenService.ObtenerExpiracionAccessToken());
    }

    public async Task RevocarRefreshTokenAsync(string refreshToken)
    {
        var token = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (token is not null)
        {
            token.Revocado = true;
            await db.SaveChangesAsync();
        }
    }

    private async Task RegistrarLogAsync(int? usuarioId, string username,
        bool exitoso, string? ip, string? plataforma, string? detalle)
    {
        db.LogsAcceso.Add(new LogAcceso
        {
            UsuarioId = usuarioId,
            UsernameUsado = username,
            Exitoso = exitoso,
            IpAddress = ip,
            Plataforma = plataforma,
            Detalle = detalle
        });
        await db.SaveChangesAsync();
    }
}