using RCD.Mob.GuardiaRelevo.Domain.Entities; 

namespace RCD.Mob.GuardiaRelevo.Application.Interfaces;

public interface IRelevoRepository
{
    Task<Relevo?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task ActualizarEstadoAsync(int id, string nuevoEstado, CancellationToken ct = default);

    // Debes tener una entidad ConfigTurno mapeada, o retornar un DTO/Tuple
    Task<ConfigTurno?> ObtenerConfigTurnoPorHoraAsync(TimeOnly horaActual, CancellationToken ct = default);

    Task<Relevo?> ObtenerPorTurnoYFechaAsync(int configTurnoId, DateTime fecha, CancellationToken ct = default);
}