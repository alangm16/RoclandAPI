using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.Web.AccesoControl.Application.DTOs;
using System.Security.Claims;

namespace RCD.Web.AccesoControl.Web.Controllers;

[ApiController]
[Route("api/web/accesocontrol/[controller]")]
[ApiExplorerSettings(GroupName = "web-accesocontrol")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>
    /// Obtiene el perfil del usuario autenticado (vía JWT de SuperAdmin).
    /// Se completa con el rol y nivel de rol extraídos del mismo token.
    /// </summary>
    [HttpGet("mi-perfil")]
    [Authorize]
    public async Task<IActionResult> ObtenerMiPerfil()
    {
        // 1. Validar que NO sea token maestro
        var esMaestro = User.FindFirst("esMaestro")?.Value == "true";
        if (esMaestro)
            return Forbid("Token maestro no válido para acceder a un proyecto específico.");

        // 2. Extraer claims necesarios
        var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(subClaim) || !int.TryParse(subClaim, out int superAdminId))
            return Unauthorized(new { mensaje = "Token inválido o sin identificador de usuario." });

        var nombreRol = User.FindFirst("nombreRol")?.Value ?? string.Empty;
        var plataforma = User.FindFirst("plataforma")?.Value ?? string.Empty;

        // 3. Validar rol y plataforma para el panel web
        var rolesPermitidos = new[] { "Gerente", "Supervisor", "Auditor" };
        if (!rolesPermitidos.Contains(nombreRol))
            return Forbid($"Rol '{nombreRol}' no tiene acceso al panel web. Roles permitidos: {string.Join(", ", rolesPermitidos)}.");

        if (plataforma != "Web")
            return Forbid($"Plataforma '{plataforma}' incorrecta. Se esperaba 'Web'.");

        // 4. Buscar perfil local en Acceso Control (solo activos)
        var perfil = await _auth.ObtenerPerfilContextoAsync(superAdminId);
        if (perfil is null)
            return StatusCode(403, new { mensaje = "No tienes un perfil activo en el módulo de Acceso Control. Contacta al administrador." });

        // 5. Obtener nivel del rol (del token)
        var nivelRolStr = User.FindFirst("nivelRol")?.Value ?? "0";
        int.TryParse(nivelRolStr, out int nivelRol);

        // 6. Retornar DTO
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