using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Domain.Interfaces;

public interface IChecklistRepository
{
    Task<List<ChecklistPunto>> ObtenerPuntosActivosAsync(CancellationToken ct = default);
    Task GuardarRespuestasAsync(List<ChecklistRespuesta> respuestas, CancellationToken ct = default);
    Task ActualizarNotasFinalesAsync(int rondinId, string? notas, CancellationToken ct = default);
}