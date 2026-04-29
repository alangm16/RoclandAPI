using RCD.Mob.GuardiaRelevo.Application.DTOs;
using RCD.Mob.GuardiaRelevo.Application.Rondines;

namespace RCD.Mob.GuardiaRelevo.Application.Interfaces;

public interface IRondinService
{
    Task<RondinActivoDto?> ObtenerActivoAsync(CancellationToken ct = default);
    Task<ValidarQRResultDto> ValidarQRAsync(ValidarQRRequestDto request, CancellationToken ct = default);
    Task<bool> FirmarAsync(FirmarRondinRequestDto request, CancellationToken ct = default);
}