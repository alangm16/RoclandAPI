using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Domain.Interfaces;

public interface IRondinRepository
{
    Task<Rondin?> ObtenerActivoAsync(DateOnly fecha, string turno, CancellationToken ct = default);
    Task<Rondin?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task ActualizarEstadoAsync(int rondinId, string estado, CancellationToken ct = default);
    Task RegistrarEventoAsync(RondinEvento evento, CancellationToken ct = default);
    Task ActualizarFechaFinAsync(int rondinId, DateTime fechaFin, CancellationToken ct = default);
}