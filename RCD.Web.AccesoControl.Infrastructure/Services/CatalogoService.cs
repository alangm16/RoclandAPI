using Microsoft.EntityFrameworkCore;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;
using RCD.Web.AccesoControl.Infrastructure.Persistence;

namespace RCD.Web.AccesoControl.Infrastructure.Services;

public class CatalogoService : ICatalogoService
{
    private readonly AccesoControlWebDbContext _db;

    public CatalogoService(AccesoControlWebDbContext db) => _db = db;

    public async Task<IEnumerable<CatalogoDto>> ObtenerAreasAsync()
    {
        return await _db.Areas
            .Where(a => a.Activo)
            .OrderBy(a => a.Nombre)
            .Select(a => new CatalogoDto(a.Id, a.Nombre))
            .ToListAsync();
    }

    public async Task<IEnumerable<CatalogoDto>> ObtenerTiposIdentificacionAsync()
    {
        return await _db.TiposIdentificacion
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .Select(t => new CatalogoDto(t.Id, t.Nombre))
            .ToListAsync();
    }

    public async Task<IEnumerable<CatalogoDto>> ObtenerMotivosVisitaAsync()
    {
        return await _db.MotivosVisita
            .Where(m => m.Activo)
            .OrderBy(m => m.Nombre)
            .Select(m => new CatalogoDto(m.Id, m.Nombre))
            .ToListAsync();
    }
}