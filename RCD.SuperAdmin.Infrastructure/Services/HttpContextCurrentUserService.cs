using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RCD.Shared.Kernel.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<HttpContextCurrentUserService> _logger;

    public HttpContextCurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<HttpContextCurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? GetUserId()
    {
        if (User == null)
        {
            _logger.LogWarning("GetUserId llamado sin HttpContext.User disponible.");
            return null;
        }

        // Buscar primero 'sub' (JwtRegisteredClaimNames.Sub)
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            // Fallback: ClaimTypes.NameIdentifier
            userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        if (string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogWarning("No se encontró claim de userId (sub ni nameid). Claims disponibles: {Claims}",
                string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
            return null;
        }

        if (int.TryParse(userIdClaim, out var userId))
            return userId;

        _logger.LogWarning("El valor del claim userId no es un entero válido: {UserIdClaim}", userIdClaim);
        return null;
    }

    public int? GetProyectoId()
    {
        // Los tokens maestros no llevan proyectoId — devuelve null intencionalmente
        var value = User?.FindFirstValue("proyectoId");
        return int.TryParse(value, out var id) ? id : null;
    }
    public string GetPlataforma()
    {
        return User?.FindFirstValue("plataforma") ?? "Web";
    }

    public bool EsTokenMaestro()
    {
        return User?.FindFirstValue("esMaestro") == "true";
    }
}