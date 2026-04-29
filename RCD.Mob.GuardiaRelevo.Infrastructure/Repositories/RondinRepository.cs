using Microsoft.EntityFrameworkCore;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using RCD.Mob.GuardiaRelevo.Domain.Interfaces;
using RCD.Mob.GuardiaRelevo.Infrastructure.Data;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Repositories;

public class RondinRepository : IRondinRepository
{
    private readonly GuardiaRelevoDbContext _db;

    public RondinRepository(GuardiaRelevoDbContext db) => _db = db;

    public Task<Rondin?> ObtenerActivoAsync(DateOnly fecha, string turno, CancellationToken ct = default)
        => _db.Rondines
              .Include(r => r.GuardiaSaliente)
              .Include(r => r.GuardiaEntrante)
              .FirstOrDefaultAsync(
                  r => r.Fecha == fecha &&
                       r.Turno == turno &&
                       (r.Estado == "Pendiente" || r.Estado == "EnCurso"), ct);

    public Task<Rondin?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => _db.Rondines
              .Include(r => r.GuardiaSaliente)
              .Include(r => r.GuardiaEntrante)
              .Include(r => r.Eventos)
              .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task ActualizarEstadoAsync(int rondinId, string estado, CancellationToken ct = default)
    {
        await _db.Rondines
                 .Where(r => r.Id == rondinId)
                 .ExecuteUpdateAsync(s => s.SetProperty(r => r.Estado, estado), ct);
    }

    public async Task RegistrarEventoAsync(RondinEvento evento, CancellationToken ct = default)
    {
        _db.RondinEventos.Add(evento);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActualizarFechaFinAsync(int rondinId, DateTime fechaFin, CancellationToken ct = default)
    {
        await _db.Rondines
                 .Where(r => r.Id == rondinId)
                 .ExecuteUpdateAsync(s => s.SetProperty(r => r.FechaFin, fechaFin), ct);
    }
}