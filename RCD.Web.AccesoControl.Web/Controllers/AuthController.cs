using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;
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
    [Authorize] // Basta con que el token sea válido para el proyecto "acceso-control"
    public async Task<IActionResult> ObtenerMiPerfil()
    {
        // 1. Extraer el ID del usuario del claim 'sub' (NameIdentifier)
        var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(subClaim) || !int.TryParse(subClaim, out int superAdminId))
            return Unauthorized(new { mensaje = "Token inválido o sin identificador de usuario." });

        // 2. Buscar el perfil local en Acceso Control
        var perfil = await _auth.ObtenerPerfilContextoAsync(superAdminId);
        if (perfil is null)
            return StatusCode(403, new { mensaje = "No tienes un perfil activo en el módulo de Acceso Control." });

        // 3. Tomar el rol del token JWT (ya viene validado por SuperAdmin)
        var nombreRol = User.FindFirst("nombreRol")?.Value ?? string.Empty;
        var nivelRolStr = User.FindFirst("nivelRol")?.Value ?? "0";
        int.TryParse(nivelRolStr, out int nivelRol);

        // 4. Retornar el DTO con todos los datos
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