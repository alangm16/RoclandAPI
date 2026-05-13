using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Seguridad;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class LogAccesoService : ILogAccesoService
{
    private readonly SuperAdminDbContext _db;

    public LogAccesoService(SuperAdminDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<LogAccesoDto>> ObtenerLogsAsync(FiltroLogsDto filtro)
    {
        const int maxTotalRegistros = 50; 

        var query = _db.LogsAcceso
            .Include(l => l.Usuario)
            .Include(l => l.Proyecto)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Username))
            query = query.Where(l => l.UsernameUsado.Contains(filtro.Username));

        if (!string.IsNullOrWhiteSpace(filtro.ProyectoCodigo))
            query = query.Where(l => l.Proyecto != null && l.Proyecto.Codigo.Contains(filtro.ProyectoCodigo));

        if (filtro.Desde.HasValue)
            query = query.Where(l => l.Fecha >= filtro.Desde.Value);

        if (filtro.Hasta.HasValue)
            query = query.Where(l => l.Fecha <= filtro.Hasta.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Plataforma))
            query = query.Where(l => l.Plataforma != null && l.Plataforma.Contains(filtro.Plataforma));

        if (filtro.Exitoso.HasValue)
            query = query.Where(l => l.Exitoso == filtro.Exitoso.Value);

        var topQuery = query
        .OrderByDescending(l => l.Fecha)
        .Take(maxTotalRegistros);  

        var total = await topQuery.CountAsync(); 

        var pageSize = filtro.TamanoPagina > maxTotalRegistros ? maxTotalRegistros : filtro.TamanoPagina;

        var items = await topQuery
            .Skip((filtro.Pagina - 1) * pageSize)
            .Take(pageSize)
                .Select(l => new LogAccesoDto(
                l.Id,
                l.UsernameUsado,
                l.Usuario != null ? l.Usuario.NombreCompleto : null,
                l.Proyecto != null ? l.Proyecto.Codigo : null,
                l.Exitoso,
                l.IpAddress,
                l.Plataforma,
                l.Detalle,
                l.Fecha
            ))
            .ToListAsync();

        return new PagedResult<LogAccesoDto>(items, total, filtro.Pagina, pageSize);
    }
}