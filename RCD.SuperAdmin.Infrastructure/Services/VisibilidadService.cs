using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.Visibilidad;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class VisibilidadService : IVisibilidadService
{
    private readonly SuperAdminDbContext _db;

    public VisibilidadService(SuperAdminDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<VistaAccesoUsuarioDto>> ObtenerVistasAccesoAsync(int usuarioId, int proyectoId)
    {
        // Validar que el usuario tenga acceso al proyecto
        var tieneAcceso = await _db.ProyectoUsuarioRoles
            .AnyAsync(pur => pur.UsuarioId == usuarioId
                          && pur.ProyectoId == proyectoId
                          && pur.Activo);

        if (!tieneAcceso)
            throw new InvalidOperationException("El usuario no tiene acceso activo a este proyecto.");

        // Traer TODAS las vistas activas del proyecto (incluyendo las no accesibles)
        var todasLasVistas = await _db.Vistas
            .Where(v => v.ProyectoId == proyectoId && v.Activo)
            .OrderBy(v => v.VistaPadreId)
            .ThenBy(v => v.Orden)
            .ToListAsync();

        // Obtener los accesos explícitos del usuario en el proyecto
        var accesos = await _db.UsuarioVistasAcceso
            .Where(uva => uva.UsuarioId == usuarioId && uva.ProyectoId == proyectoId)
            .ToDictionaryAsync(uva => uva.VistaId, uva => uva.TieneAcceso);

        return todasLasVistas.Select(v => new VistaAccesoUsuarioDto(
            v.Id,
            v.Codigo,
            v.Nombre,
            v.Ruta,
            v.Orden,
            accesos.TryGetValue(v.Id, out var tieneAcceso) ? tieneAcceso : false
        ));
    }

    public async Task ActualizarVistaAccesoAsync(int usuarioId, int vistaId, bool tieneAcceso)
    {
        // Validar que la vista exista y esté activa
        var vista = await _db.Vistas.FindAsync(vistaId)
            ?? throw new KeyNotFoundException($"Vista con Id {vistaId} no encontrada.");

        if (!vista.Activo)
            throw new InvalidOperationException("No se puede modificar el acceso a una vista inactiva.");

        // Validar que el usuario tenga acceso al proyecto de la vista
        var tieneAccesoProyecto = await _db.ProyectoUsuarioRoles
            .AnyAsync(pur => pur.UsuarioId == usuarioId
                          && pur.ProyectoId == vista.ProyectoId
                          && pur.Activo);

        if (!tieneAccesoProyecto)
            throw new InvalidOperationException("El usuario no tiene acceso activo al proyecto de esta vista.");

        // Buscar registro existente
        var acceso = await _db.UsuarioVistasAcceso
            .FirstOrDefaultAsync(uva => uva.UsuarioId == usuarioId && uva.VistaId == vistaId);

        if (acceso != null)
        {
            acceso.TieneAcceso = tieneAcceso;
            _db.UsuarioVistasAcceso.Update(acceso);
        }
        else if (tieneAcceso) // Solo crear si se concede acceso; si se deniega y no existía, es un no-op
        {
            _db.UsuarioVistasAcceso.Add(new UsuarioVistaAcceso
            {
                UsuarioId = usuarioId,
                ProyectoId = vista.ProyectoId,
                VistaId = vistaId,
                TieneAcceso = true
            });
        }

        await _db.SaveChangesAsync();
    }
}