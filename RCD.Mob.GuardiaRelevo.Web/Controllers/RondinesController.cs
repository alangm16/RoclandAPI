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
[Authorize(Policy = "GuardiaRelevoPolicy")]
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

        if (!result.Exitoso)
            return BadRequest(ApiResponse<ValidarQRResultDto>.Fail(result.Mensaje));

        return Ok(ApiResponse<ValidarQRResultDto>.Ok(result, result.Mensaje));
    }

    [HttpPost("firmar")]
    public async Task<ActionResult<ApiResponse<string>>> Firmar(
        [FromBody] FirmarRondinRequestDto request,
        CancellationToken ct)
    {
        var result = await _rondinService.FirmarAsync(request, ct);

        if (!result)
            return BadRequest(ApiResponse<string>.Fail("No se pudo registrar la firma."));

        return Ok(ApiResponse<string>.Ok("Firma registrada."));
    }
}