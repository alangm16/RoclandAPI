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
    /// Valida token directo, rol Guardia y plataforma Mobile.
    /// </summary>
    [HttpGet("mi-perfil")]
    public async Task<IActionResult> ObtenerMiPerfil()
    {
        // 1. Validar que NO sea token maestro (debe ser token directo a proyecto)
        var esMaestro = User.FindFirst("esMaestro")?.Value == "true";
        if (esMaestro)
            return Forbid("Token maestro no válido para acceder a un proyecto específico.");

        // 2. Extraer claims obligatorios
        var superAdminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("id")?.Value;
        if (!int.TryParse(superAdminIdStr, out int superAdminId))
            return Unauthorized(new { mensaje = "Token inválido o sin identificador de usuario." });

        var nombreRol = User.FindFirst("nombreRol")?.Value ?? string.Empty;
        var plataforma = User.FindFirst("plataforma")?.Value ?? string.Empty;

        // 3. Validaciones específicas para la app móvil
        if (nombreRol != "Guardia")
            return Forbid($"Rol '{nombreRol}' no autorizado. Solo los guardias pueden usar la app móvil.");

        if (plataforma != "Mobile")
            return Forbid($"Plataforma '{plataforma}' incorrecta. Se esperaba 'Mobile'.");

        // 4. Obtener perfil local desde la base de datos de Acceso Control
        var perfil = await _auth.ObtenerPerfilContextoAsync(superAdminId);
        if (perfil is null)
            return StatusCode(403, new { mensaje = "No tienes un perfil activo en Acceso Control. Contacta al administrador." });

        var nivelRolStr = User.FindFirst("nivelRol")?.Value ?? "0";
        int.TryParse(nivelRolStr, out int nivelRol);

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