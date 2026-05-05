using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Application.Interfaces;

public interface IUsuarioRepository
{
    // Cambiamos Usuario por NumeroEmpleado
    Task<Usuario?> ObtenerPorNumeroEmpleadoAsync(string numeroEmpleado, CancellationToken ct = default);

    // Ajustamos el nombre del parámetro para ser más claros
    Task<Usuario?> ObtenerPorIdAsync(int superAdminUsuarioId, CancellationToken ct = default);
}