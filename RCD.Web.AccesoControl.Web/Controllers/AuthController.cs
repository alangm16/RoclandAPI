//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using RCD.Web.AccesoControl.Application.Interfaces;
//using System.Security.Claims;

//namespace RCD.Web.AccesoControl.Web.Controllers;

//[ApiController]
//[Route("api/web/accesocontrol/[controller]")]
//[ApiExplorerSettings(GroupName = "web-accesocontrol")]
//[Authorize] // ── FIX: Ahora requiere el token generado por SuperAdmin
//public class AuthController : ControllerBase
//{
//    private readonly IAuthService _auth;

//    public AuthController(IAuthService auth)
//    {
//        _auth = auth;
//    }

//    /// <summary>
//    /// Este endpoint reemplaza a los antiguos LoginGuardia y LoginAdmin.
//    /// Valida si el usuario autenticado en SuperAdmin tiene un perfil en Acceso Control.
//    /// </summary>
//    [HttpGet("mi-perfil")]
//    public async Task<IActionResult> ObtenerMiPerfil()
//    {
//        // 1. Extraer el ID del usuario de los Claims del Token JWT
//        var superAdminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
//                              ?? User.FindFirst("id")?.Value;

//        if (!int.TryParse(superAdminIdStr, out int superAdminId))
//            return Unauthorized(new { mensaje = "El token no contiene un ID de usuario válido." });

//        // 2. Consultar el Perfil en la tabla unificada (Perfiles)
//        var perfil = await _auth.ObtenerPerfilPorSuperAdminIdAsync(superAdminId);

//        if (perfil == null || !perfil.Activo)
//            return StatusCode(403, new { mensaje = "No tienes un perfil configurado o activo en el módulo de Acceso Control." });

//        return Ok(perfil);
//    }

//    // Se eliminó GenerarHash porque la gestión de contraseñas ahora es responsabilidad de SuperAdmin.
//}