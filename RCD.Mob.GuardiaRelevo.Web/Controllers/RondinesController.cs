using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.Mob.GuardiaRelevo.Application.Common;
using RCD.Mob.GuardiaRelevo.Application.DTOs;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using RCD.Mob.GuardiaRelevo.Application.Rondines;

namespace RCD.Mob.GuardiaRelevo.Web.Controllers;

[ApiController]
[Route("api/mob/guardiarelevo/rondines")]
[ApiExplorerSettings(GroupName = "mobile-guardia-relevo")]
[Authorize(Policy = "GuardiaRelevoPolicy")] // Ojo: Asegúrate de que esta policy exista en tu Program.cs / IoC
public class RondinesController : ControllerBase
{
    private readonly IRondinService _rondinService;

    public RondinesController(IRondinService rondinService) => _rondinService = rondinService;

    [HttpGet("activo")]
    public async Task<ActionResult<ApiResponse<RondinActivoDto>>> ObtenerActivo(CancellationToken ct)
    {
        var result = await _rondinService.ObtenerActivoAsync(ct);

        if (result is null)
            return Ok(ApiResponse<RondinActivoDto>.Fail(
                "No hay rondín disponible en este momento."));

        return Ok(ApiResponse<RondinActivoDto>.Ok(result));
    }

    [HttpPost("validar-qr")]
    public async Task<ActionResult<ApiResponse<ValidarQRResultDto>>> ValidarQR(
        [FromBody] ValidarQRRequestDto request,
        CancellationToken ct)
    {
        var result = await _rondinService.ValidarQRAsync(request, ct);

        // 1. Cambiamos result.Exitoso por result.Exito
        if (!result.Exito)
            return BadRequest(ApiResponse<ValidarQRResultDto>.Fail(result.Mensaje));

        return Ok(ApiResponse<ValidarQRResultDto>.Ok(result, result.Mensaje));
    }

    // 2. Cambiamos la ruta y la lógica, ya no pedimos firma en Base64
    [HttpPost("finalizar/{rondinId}")]
    public async Task<ActionResult<ApiResponse<string>>> Finalizar(
        int rondinId,
        CancellationToken ct)
    {
        // Llamamos al nuevo método que creamos en el servicio
        var result = await _rondinService.FinalizarRondinAsync(rondinId, ct);

        if (!result)
            return BadRequest(ApiResponse<string>.Fail("No se pudo finalizar el rondín o ya estaba cerrado."));

        return Ok(ApiResponse<string>.Ok("Rondín finalizado con éxito."));
    }
}