using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.Interfaces;
using System.Security.Claims;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet("{proyectoId:int}")]
    public async Task<IActionResult> ObtenerMenu(int proyectoId)
    {
        // 1. Extraer el ID del usuario desde el token JWT
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int usuarioId))
            return Unauthorized(new { mensaje = "Sesión inválida o expirada." });

        // 2. Extraer contexto de seguridad
        var esMaestro = User.FindFirst("esMaestro")?.Value == "true";
        var tokenProyectoIdStr = User.FindFirst("proyectoId")?.Value;
        int.TryParse(tokenProyectoIdStr, out int tokenProyectoId);

        // 3. Validación cross‑project
        if (!esMaestro && tokenProyectoId != proyectoId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { mensaje = "Acceso denegado: El token actual no pertenece al proyecto solicitado." });
        }

        // 4. Obtener el menú
        var menu = await _menuService.ObtenerMenuAsync(usuarioId, proyectoId);
        return Ok(menu);
    }
}