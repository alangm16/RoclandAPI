using Microsoft.EntityFrameworkCore;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using RCD.Mob.GuardiaRelevo.Infrastructure.Data;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Repositories;

public class RondinRepository : IRondinRepository
{
    private readonly GuardiaRelevoDbContext _db;

    public RondinRepository(GuardiaRelevoDbContext db) => _db = db;

    public Task<Rondin?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => _db.Rondines
              // Si en el futuro necesitas los datos del guardia que hizo este rondin específico, 
              // puedes descomentar la siguiente línea (si agregaste la propiedad de navegación en Rondin.cs):
              // .Include(r => r.Guardia) 
              .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task ActualizarEstadoAsync(int rondinId, string estado, CancellationToken ct = default)
    {
        await _db.Rondines
                 .Where(r => r.Id == rondinId)
                 .ExecuteUpdateAsync(s => s.SetProperty(r => r.Estado, estado), ct);
    }

    public async Task ActualizarFechaFinAsync(int rondinId, DateTime fechaFin, CancellationToken ct = default)
    {
        await _db.Rondines
                 .Where(r => r.Id == rondinId)
                 .ExecuteUpdateAsync(s => s.SetProperty(r => r.FechaFin, fechaFin), ct);
    }

    public async Task ActualizarAsync(Rondin rondin, CancellationToken ct = default)
    {
        _db.Rondines.Update(rondin);
        await _db.SaveChangesAsync(ct);
    }
}