using Microsoft.EntityFrameworkCore;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using RCD.Mob.GuardiaRelevo.Infrastructure.Data;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Repositories;

public class RelevoRepository : IRelevoRepository
{
    private readonly GuardiaRelevoDbContext _db;

    public RelevoRepository(GuardiaRelevoDbContext db)
    {
        _db = db;
    }

    public async Task<Relevo?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Relevos
            // Opcional: .Include(r => r.Rondines) si en el futuro necesitas cargar sus rondines
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task ActualizarEstadoAsync(int id, string nuevoEstado, CancellationToken ct = default)
    {
        var relevo = await _db.Relevos.FindAsync(new object[] { id }, ct);
        if (relevo != null)
        {
            relevo.Estado = nuevoEstado;
            relevo.FechaModificacion = DateTime.Now;

            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<ConfigTurno?> ObtenerConfigTurnoPorHoraAsync(TimeOnly horaActual, CancellationToken ct = default)
    {
        // Convertimos el TimeOnly a TimeSpan para que SQL Server lo entienda sin problemas
        var hora = horaActual.ToTimeSpan();

        // Buscamos el turno donde la hora actual caiga dentro de la ventana de inicio y fin
        return await _db.ConfigTurnos
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Activo && hora >= t.HoraInicioVentana && hora <= t.HoraFinVentana, ct);
    }

    public async Task<Relevo?> ObtenerPorTurnoYFechaAsync(int configTurnoId, DateTime fecha, CancellationToken ct = default)
    {
        return await _db.Relevos
            .Include(r => r.GuardiaSaliente) // Hacemos el JOIN para traer los datos del guardia
            .Include(r => r.GuardiaEntrante)
            .FirstOrDefaultAsync(r => r.ConfigTurnoId == configTurnoId && r.Fecha == fecha.Date, ct);
    }
}
