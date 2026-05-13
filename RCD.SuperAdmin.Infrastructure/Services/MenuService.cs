// RCD.SuperAdmin.Infrastructure/Services/MenuService.cs
using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.Menu;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
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
        // ── 1. Verificar que el usuario tiene rol activo en el proyecto ───────
        var tieneAccesoProyecto = await _db.ProyectoUsuarioRoles
            .AnyAsync(pur => pur.UsuarioId == usuarioId
                          && pur.ProyectoId == proyectoId
                          && pur.Activo);

        if (!tieneAccesoProyecto)
            return [];

        // ── 2. Traer TODAS las vistas activas del proyecto (planas) ───────────
        var todasLasVistas = await _db.Vistas
            .Where(v => v.ProyectoId == proyectoId && v.Activo)
            .OrderBy(v => v.Orden)
            .ToListAsync();

        // ── 3. IDs con acceso explícito ───────────────────────────────────────
        var idsConAcceso = await _db.UsuarioVistasAcceso
            .Where(uva => uva.UsuarioId == usuarioId
                       && uva.ProyectoId == proyectoId
                       && uva.TieneAcceso)
            .Select(uva => uva.VistaId)
            .ToHashSetAsync();

        // ── 4. Construir árbol desde las raíces ───────────────────────────────
        return ConstruirNivel(todasLasVistas, padreId: null, idsConAcceso);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Recursión pura: toma los hijos directos de `padreId`, resuelve sus
    // propios hijos antes de decidir si el nodo es visible, y devuelve
    // solo los nodos que deben aparecer en el sidebar.
    // ─────────────────────────────────────────────────────────────────────────
    private static List<VistaMenuDto> ConstruirNivel(
        List<Vista> todas,
        int? padreId,
        HashSet<int> idsConAcceso)
    {
        var nivel = new List<VistaMenuDto>();

        var hijosDirectos = todas
            .Where(v => v.VistaPadreId == padreId)
            .OrderBy(v => v.Orden);

        foreach (var vista in hijosDirectos)
        {
            if (vista.EsContenedor)
            {
                // Primero resolvemos los hijos; solo mostramos el contenedor
                // si tiene al menos un hijo visible (en cualquier nivel).
                var hijos = ConstruirNivel(todas, vista.Id, idsConAcceso);

                if (hijos.Count > 0)
                {
                    nivel.Add(new VistaMenuDto(
                        Id: vista.Id,
                        Codigo: vista.Codigo,
                        Nombre: vista.Nombre,
                        Ruta: vista.Ruta,
                        Icono: vista.Icono,
                        Orden: vista.Orden,
                        EsContenedor: true,
                        Hijos: hijos
                    ));
                }
            }
            else
            {
                // Ítem navegable: solo si el usuario tiene acceso explícito
                if (idsConAcceso.Contains(vista.Id))
                {
                    nivel.Add(new VistaMenuDto(
                        Id: vista.Id,
                        Codigo: vista.Codigo,
                        Nombre: vista.Nombre,
                        Ruta: vista.Ruta,
                        Icono: vista.Icono,
                        Orden: vista.Orden,
                        EsContenedor: false,
                        Hijos: []
                    ));
                }
            }
        }

        return nivel;
    }
}