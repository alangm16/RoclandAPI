using Microsoft.EntityFrameworkCore;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using RCD.Mob.GuardiaRelevo.Domain.Interfaces;
using RCD.Mob.GuardiaRelevo.Infrastructure.Data;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly GuardiaRelevoDbContext _db;

    public UsuarioRepository(GuardiaRelevoDbContext db) => _db = db;

    public Task<Usuario?> ObtenerPorUsuarioAsync(string usuario, CancellationToken ct = default)
        => _db.Usuarios
              .AsNoTracking()
              .FirstOrDefaultAsync(u => u.Usuario_ == usuario && u.Activo, ct);

    public Task<Usuario?> ObtenerPorQRAsync(string qrCode, CancellationToken ct = default)
        => _db.Usuarios
              .AsNoTracking()
              .FirstOrDefaultAsync(u => u.QRCode == qrCode && u.Activo, ct);

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => _db.Usuarios
              .AsNoTracking()
              .FirstOrDefaultAsync(u => u.Id == id && u.Activo, ct);
}