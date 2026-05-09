using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.Shared.Kernel.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
[Authorize] // Cualquier usuario logueado en cualquier proyecto puede pedir SU menú
public class MenuController(
    IMenuService menuService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("{proyectoId:int}")]
    public async Task<IActionResult> ObtenerMenu(int proyectoId)
    {
        // 1. Identificar al usuario desde el Token JWT
        var usuarioId = currentUserService.GetUserId();
        if (usuarioId is null)
            return Unauthorized(new { mensaje = "Sesión inválida o expirada." });

        // 2. Extraer contexto de seguridad del Token
        var esMaestro = currentUserService.EsTokenMaestro();
        var tokenProyectoId = currentUserService.GetProyectoId();

        // 3. Validación de seguridad (Cross-Project Prevention)
        // Si no es un token del orquestador (maestro), solo puede consultar el menú de la app a la que se logueó
        if (!esMaestro && tokenProyectoId != proyectoId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { mensaje = "Acceso denegado: El token actual no pertenece al proyecto solicitado." });
        }

        // 4. Obtener el menú dinámico basado en la BD
        var menu = await menuService.ObtenerMenuAsync(usuarioId.Value, proyectoId);

        return Ok(menu);
    }
}