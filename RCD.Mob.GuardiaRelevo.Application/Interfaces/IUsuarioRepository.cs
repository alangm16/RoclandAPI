using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorUsuarioAsync(string usuario, CancellationToken ct = default);
    Task<Usuario?> ObtenerPorQRAsync(string qrCode, CancellationToken ct = default);
    Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
}