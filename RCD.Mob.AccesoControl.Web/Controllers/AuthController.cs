using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;
using System.Security.Claims;

namespace RCD.Mob.AccesoControl.Web.Controllers;

[ApiController]
[Route("api/mob/accesocontrol/[controller]")]
[ApiExplorerSettings(GroupName = "mobile-accesocontrol")]
[Authorize] // ── FIX: Exige el token generado por SuperAdmin
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>
    /// Reemplaza al antiguo LoginGuardia.
    /// Valida el JWT del SuperAdmin y devuelve el perfil del guardia si está activo.
    /// </summary>
    [HttpGet("mi-perfil")]
    public async Task<IActionResult> ObtenerMiPerfil()
    {
        // 1. Extraer el ID del usuario de los Claims del Token JWT
        var superAdminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("id")?.Value;

        if (!int.TryParse(superAdminIdStr, out int superAdminId))
            return Unauthorized(new { mensaje = "El token no contiene un ID de usuario válido." });

        // 2. Consultar el Perfil en la tabla unificada
        var perfil = await _auth.ObtenerPerfilPorSuperAdminIdAsync(superAdminId);

        if (perfil == null || !perfil.Activo)
            return StatusCode(403, new { mensaje = "No tienes un perfil activo asignado en el sistema de Control de Acceso." });

        // Opcional: Validar que solo Guardias/Supervisores se logueen en la app móvil
        if (perfil.TipoPerfil != "Guardia" && perfil.TipoPerfil != "Supervisor" && perfil.TipoPerfil != "Administrador")
            return StatusCode(403, new { mensaje = "Tu perfil no tiene permisos para usar la aplicación móvil." });

        return Ok(perfil);
    }
}