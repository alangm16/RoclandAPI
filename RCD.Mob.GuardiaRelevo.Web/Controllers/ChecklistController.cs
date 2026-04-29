using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.Mob.GuardiaRelevo.Application.Common;
using RCD.Mob.GuardiaRelevo.Application.DTOs;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;

namespace RCD.Mob.GuardiaRelevo.Web.Controllers;

[ApiController]
[Route("api/mob/guardiarelevo/checklist")]
[ApiExplorerSettings(GroupName = "mobile-guardia-relevo")]
[Authorize(Policy = "GuardiaRelevoPolicy")]
public class ChecklistController : ControllerBase
{
    private readonly IChecklistService _checklistService;

    public ChecklistController(IChecklistService checklistService) => _checklistService = checklistService;

    [HttpGet("puntos")]
    public async Task<ActionResult<ApiResponse<List<ChecklistCategoriaDto>>>> ObtenerPuntos(CancellationToken ct)
    {
        var result = await _checklistService.ObtenerPuntosAsync(ct);
        return Ok(ApiResponse<List<ChecklistCategoriaDto>>.Ok(result));
    }

    [HttpPost("respuestas")]
    public async Task<ActionResult<ApiResponse<string>>> GuardarRespuestas(
        [FromBody] GuardarRespuestasRequestDto request,
        CancellationToken ct)
    {
        var result = await _checklistService.GuardarRespuestasAsync(request, ct);

        if (!result)
            return BadRequest(ApiResponse<string>.Fail("No se pudieron guardar las respuestas."));

        return Ok(ApiResponse<string>.Ok("Respuestas guardadas."));
    }
}