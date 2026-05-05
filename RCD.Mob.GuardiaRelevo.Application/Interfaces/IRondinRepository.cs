using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Application.Interfaces;

public interface IRondinRepository
{
    Task<Rondin?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task ActualizarEstadoAsync(int rondinId, string estado, CancellationToken ct = default);
    Task ActualizarFechaFinAsync(int rondinId, DateTime fechaFin, CancellationToken ct = default);
    Task ActualizarAsync(Rondin rondin, CancellationToken ct = default);
}