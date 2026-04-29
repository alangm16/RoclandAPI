using Microsoft.EntityFrameworkCore;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using RCD.Mob.GuardiaRelevo.Domain.Interfaces;
using RCD.Mob.GuardiaRelevo.Infrastructure.Data;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Repositories;

public class ChecklistRepository : IChecklistRepository
{
    private readonly GuardiaRelevoDbContext _db;

    public ChecklistRepository(GuardiaRelevoDbContext db) => _db = db;

    public Task<List<ChecklistPunto>> ObtenerPuntosActivosAsync(CancellationToken ct = default)
        => _db.ChecklistPuntos
              .AsNoTracking()
              .Where(p => p.Activo)
              .OrderBy(p => p.Categoria)
              .ThenBy(p => p.Orden)
              .ToListAsync(ct);

    public async Task GuardarRespuestasAsync(List<ChecklistRespuesta> respuestas, CancellationToken ct = default)
    {
        _db.ChecklistRespuestas.AddRange(respuestas);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActualizarNotasFinalesAsync(int rondinId, string? notas, CancellationToken ct = default)
    {
        await _db.Rondines
                 .Where(r => r.Id == rondinId)
                 .ExecuteUpdateAsync(s => s.SetProperty(r => r.NotasFinales, notas), ct);
    }
}