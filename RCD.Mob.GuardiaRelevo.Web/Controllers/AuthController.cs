using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using System.Security.Claims;

namespace RCD.Mob.GuardiaRelevo.Web.Controllers;

[ApiController]
[Route("api/mob/guardiarelevo/[controller]")]
[ApiExplorerSettings(GroupName = "mobile-guardia-relevo")]
[Authorize(Policy = "GuardiaRelevoPolicy")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>
    /// Valida el JWT del SuperAdmin y devuelve el perfil del guardia para el módulo de Relevo.
    /// Incluye validaciones de rol, plataforma y tipo de token.
    /// </summary>
    [HttpGet("mi-perfil")]
    public async Task<IActionResult> ObtenerMiPerfil()
    {
        // 1. Validar que NO sea token maestro (debe ser token directo a proyecto)
        var esMaestro = User.FindFirst("esMaestro")?.Value == "true";
        if (esMaestro)
            return Forbid("Token maestro no válido para acceder a un proyecto específico.");

        // 2. Extraer claims obligatorios
        var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("id")?.Value;
        if (!int.TryParse(subClaim, out int superAdminId))
            return Unauthorized(new { mensaje = "Token inválido o sin identificador de usuario." });

        var nombreRol = User.FindFirst("nombreRol")?.Value ?? string.Empty;
        var plataforma = User.FindFirst("plataforma")?.Value ?? string.Empty;

        // 3. Validaciones específicas para la app móvil
        if (nombreRol != "Guardia")
            return Forbid($"Rol '{nombreRol}' no autorizado. Solo los guardias pueden usar esta app.");

        if (plataforma != "Mobile")
            return Forbid($"Plataforma '{plataforma}' incorrecta. Se esperaba 'Mobile'.");

        // 4. Consultar el perfil local en la tabla de Relevo
        var perfil = await _auth.ObtenerPerfilPorSuperAdminIdAsync(superAdminId);

        if (perfil == null || !perfil.Activo)
            return StatusCode(403, new { mensaje = "No tienes un perfil configurado o activo en la app de Relevo. Contacta al administrador." });

        // Opcional: si el DTO del perfil no contiene el rol, se puede agregar aquí,
        // pero normalmente el perfil local ya sabe que es Guardia (por el contexto).
        // Si se requiere devolver el rol, se puede mapear en el DTO.
        return Ok(perfil);
    }
}