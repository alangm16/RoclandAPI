using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.Menu;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class MenuService : IMenuService
{
    private readonly SuperAdminDbContext _db;

    public MenuService(SuperAdminDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<VistaMenuDto>> ObtenerMenuAsync(int usuarioId, int proyectoId)
    {
        // 1. Verificar que el usuario tenga un rol activo en el proyecto
        var tieneAccesoProyecto = await _db.ProyectoUsuarioRoles
            .AnyAsync(pur => pur.UsuarioId == usuarioId
                          && pur.ProyectoId == proyectoId
                          && pur.Activo);

        if (!tieneAccesoProyecto)
            return Enumerable.Empty<VistaMenuDto>();

        // 2. Obtener todas las vistas activas del proyecto (ordenadas)
        var vistasProyecto = await _db.Vistas
            .Where(v => v.ProyectoId == proyectoId && v.Activo)
            .OrderBy(v => v.Orden)
            .ToListAsync();

        // 3. Obtener los ids de vistas que el usuario tiene permitidas explícitamente
        var vistaIdsPermitidas = await _db.UsuarioVistasAcceso
            .Where(uva => uva.UsuarioId == usuarioId
                       && uva.ProyectoId == proyectoId
                       && uva.TieneAcceso)
            .Select(uva => uva.VistaId)
            .ToListAsync();

        // 4. Si no hay ningún registro de acceso explícito se aplica “denegación por defecto”
        //    (solo se muestran las vistas que tengan TieneAcceso = true)
        var resultado = vistasProyecto
            .Where(v => vistaIdsPermitidas.Contains(v.Id))
            .Select(v => new VistaMenuDto(
                v.Id,
                v.Codigo,
                v.Nombre,
                v.Ruta,
                v.Icono,
                v.Orden
            ))
            .ToList();

        return resultado;
    }
}