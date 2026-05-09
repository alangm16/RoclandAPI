using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public int? GetUserId()
    {
        var value = User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(value, out var id) ? id : null;
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