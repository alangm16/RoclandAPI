using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using System.Security.Claims;

namespace RCD.Mob.GuardiaRelevo.Web.Controllers;

[ApiController]
[Route("api/mob/guardiarelevo/[controller]")]
[ApiExplorerSettings(GroupName = "mobile-guardia-relevo")]
[Authorize] // ── Exige el token generado por SuperAdmin
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>
    /// Valida el JWT del SuperAdmin y devuelve el perfil del guardia para el módulo de Relevo.
    /// </summary>
    [HttpGet("mi-perfil")]
    public async Task<IActionResult> ObtenerMiPerfil()
    {
        // 1. Extraer el ID del usuario de los Claims del Token JWT
        var superAdminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("id")?.Value;

        if (!int.TryParse(superAdminIdStr, out int superAdminId))
            return Unauthorized(new { mensaje = "El token no contiene un ID de usuario válido." });

        // 2. Consultar el Perfil en la tabla local de Relevos
        var perfil = await _auth.ObtenerPerfilPorSuperAdminIdAsync(superAdminId);

        if (perfil == null || !perfil.Activo)
            return StatusCode(403, new { mensaje = "No tienes un perfil configurado o activo en la app de Relevo." });

        return Ok(perfil);
    }
}