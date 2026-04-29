using RCD.Mob.GuardiaRelevo.Application.DTOs;

namespace RCD.Mob.GuardiaRelevo.Application.Interfaces;

public interface IChecklistService
{
    Task<List<ChecklistCategoriaDto>> ObtenerPuntosAsync(CancellationToken ct = default);
    Task<bool> GuardarRespuestasAsync(GuardarRespuestasRequestDto request, CancellationToken ct = default);
}