using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;
using RCD.Web.AccesoControl.Infrastructure.Persistence;

namespace RCD.Web.AccesoControl.Infrastructure.Services;

public class CatalogoService : ICatalogoService
{
    private readonly AccesoControlWebDbContext _db;
    private readonly IMemoryCache _cache;

    // Definición de las llaves de caché
    private const string CacheKeyAreas = "Catalogos_Areas";
    private const string CacheKeyTiposId = "Catalogos_TiposId";
    private const string CacheKeyMotivos = "Catalogos_Motivos";

    // Tiempo de vida del caché
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(8);

    public CatalogoService(AccesoControlWebDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IEnumerable<CatalogoDto>> ObtenerAreasAsync()
    {
        // 1. Intentar obtener los datos desde el caché
        if (!_cache.TryGetValue(CacheKeyAreas, out IEnumerable<CatalogoDto>? areas))
        {
            // 2. Si no están en caché, consultar a la base de datos
            areas = await _db.Areas
                .Where(a => a.Activo)
                .OrderBy(a => a.Nombre)
                .Select(a => new CatalogoDto(a.Id, a.Nombre))
                .ToListAsync();

            // 3. Guardar en caché con el tiempo de expiración definido
            _cache.Set(CacheKeyAreas, areas, _cacheDuration);
        }

        return areas ?? Array.Empty<CatalogoDto>();
    }

    public async Task<IEnumerable<CatalogoDto>> ObtenerTiposIdentificacionAsync()
    {
        if (!_cache.TryGetValue(CacheKeyTiposId, out IEnumerable<CatalogoDto>? tipos))
        {
            tipos = await _db.TiposIdentificacion
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .Select(t => new CatalogoDto(t.Id, t.Nombre))
                .ToListAsync();

            _cache.Set(CacheKeyTiposId, tipos, _cacheDuration);
        }

        return tipos ?? Array.Empty<CatalogoDto>();
    }

    public async Task<IEnumerable<CatalogoDto>> ObtenerMotivosVisitaAsync()
    {
        if (!_cache.TryGetValue(CacheKeyMotivos, out IEnumerable<CatalogoDto>? motivos))
        {
            motivos = await _db.MotivosVisita
                .Where(m => m.Activo)
                .OrderBy(m => m.Nombre)
                .Select(m => new CatalogoDto(m.Id, m.Nombre))
                .ToListAsync();

            _cache.Set(CacheKeyMotivos, motivos, _cacheDuration);
        }

        return motivos ?? Array.Empty<CatalogoDto>();
    }
}