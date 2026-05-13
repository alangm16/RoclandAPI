using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Alertas;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class AlertaService : IAlertaService
{
    private readonly SuperAdminDbContext _db;

    public AlertaService(SuperAdminDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AlertaDto>> ObtenerAlertasAsync(FiltroAlertasDto filtro)
    {
        var query = _db.Alertas
            .Include(a => a.Proyecto)
            .AsQueryable();

        if (filtro.ProyectoId.HasValue)
            query = query.Where(a => a.ProyectoId == filtro.ProyectoId.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Tipo))
            query = query.Where(a => a.Tipo == filtro.Tipo);

        if (filtro.Resuelta.HasValue)
            query = query.Where(a => a.Resuelta == filtro.Resuelta.Value);

        if (filtro.Desde.HasValue)
            query = query.Where(a => a.Fecha >= filtro.Desde.Value);

        if (filtro.Hasta.HasValue)
            query = query.Where(a => a.Fecha <= filtro.Hasta.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Fecha)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(a => new AlertaDto(
                a.Id,
                a.ProyectoId,
                a.Proyecto != null ? a.Proyecto.Codigo : null,
                a.Tipo,
                a.Titulo,
                a.Mensaje,
                a.Fecha,
                a.Resuelta,
                a.AccionUrl
            ))
            .ToListAsync();

        return new PagedResult<AlertaDto>(
            items,
            total,
            filtro.Pagina,
            filtro.TamanoPagina
        );
    }

    public async Task MarcarResueltaAsync(int alertaId)
    {
        var alerta = await _db.Alertas.FindAsync(alertaId)
            ?? throw new KeyNotFoundException($"Alerta con Id {alertaId} no encontrada.");

        if (alerta.Resuelta)
            throw new InvalidOperationException("La alerta ya está marcada como resuelta.");

        alerta.Resuelta = true;
        _db.Alertas.Update(alerta);
        await _db.SaveChangesAsync();
    }
}