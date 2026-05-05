using Microsoft.EntityFrameworkCore;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using RCD.Mob.GuardiaRelevo.Infrastructure.Data;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly GuardiaRelevoDbContext _db;

    public UsuarioRepository(GuardiaRelevoDbContext db) => _db = db;

    public Task<Usuario?> ObtenerPorNumeroEmpleadoAsync(string numeroEmpleado, CancellationToken ct = default)
        => _db.Usuarios
              .AsNoTracking()
              .FirstOrDefaultAsync(u => u.NumeroEmpleado == numeroEmpleado && u.Activo, ct);

    public Task<Usuario?> ObtenerPorIdAsync(int superAdminUsuarioId, CancellationToken ct = default)
        => _db.Usuarios
              .AsNoTracking()
              .FirstOrDefaultAsync(u => u.SuperAdminUsuarioId == superAdminUsuarioId && u.Activo, ct);
}