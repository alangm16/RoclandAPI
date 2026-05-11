using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;
using System.Security.Claims;

namespace RCD.Mob.AccesoControl.Web.Controllers;

[ApiController]
[Route("api/mob/accesocontrol/[controller]")]
[ApiExplorerSettings(GroupName = "mobile-accesocontrol")]
[Authorize(Policy = "AccesoControlMobilePolicy")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService auth, ILogger<AuthController> logger)
    {
        _auth = auth;
        _logger = logger;
    }

    /// <summary>
    /// Devuelve el perfil del guardia autenticado, completado con el rol del JWT.
    /// </summary>
    [HttpGet("mi-perfil")]
    public async Task<IActionResult> ObtenerMiPerfil()
    {
        var superAdminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("id")?.Value;
        if (!int.TryParse(superAdminIdStr, out int superAdminId))
            return Unauthorized(new { mensaje = "Token inválido." });

        var perfil = await _auth.ObtenerPerfilContextoAsync(superAdminId);
        if (perfil is null)
            return StatusCode(403, new { mensaje = "No tienes un perfil activo en Acceso Control." });

        var nombreRol = User.FindFirst("nombreRol")?.Value ?? string.Empty;
        var nivelRolStr = User.FindFirst("nivelRol")?.Value ?? "0";
        int.TryParse(nivelRolStr, out int nivelRol);

        // Si se requiere, se puede forzar a que solo Guardias accedan (la política ya lo asegura)
        if (nombreRol != "Guardia")
            return StatusCode(403, new { mensaje = "Solo los guardias pueden usar la app móvil." });

        return Ok(new PerfilContextoDto(
            PerfilId: perfil.PerfilId,
            SuperAdminUsuarioId: perfil.SuperAdminUsuarioId,
            NombreCompleto: perfil.NombreCompleto,
            NombreRol: nombreRol,
            NivelRol: nivelRol,
            Turno: perfil.Turno,
            NumeroEmpleado: perfil.NumeroEmpleado
        ));
    }
}