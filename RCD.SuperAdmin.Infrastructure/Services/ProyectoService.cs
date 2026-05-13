using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.Proyectos;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class ProyectoService : IProyectoService
{
    private readonly SuperAdminDbContext _db;

    public ProyectoService(SuperAdminDbContext db)
    {
        _db = db;
    }

    // ── Proyectos ────────────────────────────────────────────────────────────

    public async Task<IEnumerable<ProyectoListDto>> ObtenerTodosAsync()
    {
        return await _db.Proyectos
            .OrderBy(p => p.Orden)
            .ThenBy(p => p.Nombre)
            .Select(p => new ProyectoListDto(
                p.Id, p.Codigo, p.Nombre, p.Plataforma,
                p.IconoCss, p.Estado, p.Version, p.Orden, p.Activo))
            .ToListAsync();
    }

    public async Task<ProyectoDetalleDto?> ObtenerPorIdAsync(int id)
    {
        var proyecto = await _db.Proyectos
            .Include(p => p.Roles)
            .Include(p => p.Vistas)
            .FirstOrDefaultAsync(p => p.Id == id);

        return proyecto is null ? null : MapProyectoDetalle(proyecto);
    }

    public async Task<ProyectoDetalleDto> CrearAsync(CrearProyectoDto dto)
    {
        if (await _db.Proyectos.AnyAsync(p => p.Codigo == dto.Codigo))
            throw new InvalidOperationException($"El código '{dto.Codigo}' ya está en uso.");

        ValidarPlataforma(dto.Plataforma);

        var proyecto = new Proyecto
        {
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Plataforma = dto.Plataforma,
            IconoCss = dto.IconoCss,
            UrlBase = dto.UrlBase,
            Version = dto.Version ?? "1.0.0",
            Estado = "Produccion",
            Descripcion = dto.Descripcion,
            Orden = dto.Orden,
            Activo = true
        };

        _db.Proyectos.Add(proyecto);
        await _db.SaveChangesAsync();

        return new ProyectoDetalleDto(
            proyecto.Id, proyecto.Codigo, proyecto.Nombre, proyecto.Plataforma,
            proyecto.IconoCss, proyecto.UrlBase, proyecto.Estado, proyecto.Version,
            proyecto.Descripcion, proyecto.Orden, proyecto.Activo,
            [], []);
    }

    public async Task<ProyectoDetalleDto> ActualizarAsync(int id, ActualizarProyectoDto dto)
    {
        var proyecto = await _db.Proyectos.FindAsync(id)
            ?? throw new KeyNotFoundException($"Proyecto con Id {id} no encontrado.");

        ValidarPlataforma(dto.Plataforma);
        ValidarEstado(dto.Estado);

        proyecto.Nombre = dto.Nombre;
        proyecto.Plataforma = dto.Plataforma;
        proyecto.IconoCss = dto.IconoCss;
        proyecto.UrlBase = dto.UrlBase;
        proyecto.Estado = dto.Estado;
        proyecto.Version = dto.Version;
        proyecto.Descripcion = dto.Descripcion;
        proyecto.Orden = dto.Orden;

        _db.Proyectos.Update(proyecto);
        await _db.SaveChangesAsync();

        await _db.Entry(proyecto).Collection(p => p.Roles).LoadAsync();
        await _db.Entry(proyecto).Collection(p => p.Vistas).LoadAsync();

        return MapProyectoDetalle(proyecto);
    }

    public async Task DesactivarAsync(int id)
    {
        var proyecto = await _db.Proyectos.FindAsync(id)
            ?? throw new KeyNotFoundException($"Proyecto con Id {id} no encontrado.");

        proyecto.Activo = false;
        _db.Proyectos.Update(proyecto);
        await _db.SaveChangesAsync();
    }

    // ── Roles ────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<RolDto>> ObtenerRolesAsync(int proyectoId)
    {
        await ValidarProyectoExisteAsync(proyectoId);

        return await _db.Roles
            .Where(r => r.ProyectoId == proyectoId && r.Activo)
            .OrderBy(r => r.Nivel).ThenBy(r => r.Nombre)
            .Select(r => new RolDto(r.Id, r.Nombre, r.Nivel, r.Descripcion, r.Activo))
            .ToListAsync();
    }

    public async Task<RolDto> CrearRolAsync(int proyectoId, CrearRolDto dto)
    {
        await ValidarProyectoExisteAsync(proyectoId);

        if (await _db.Roles.AnyAsync(r => r.ProyectoId == proyectoId && r.Nombre == dto.Nombre))
            throw new InvalidOperationException($"Ya existe un rol '{dto.Nombre}' en este proyecto.");

        var rol = new Rol
        {
            ProyectoId = proyectoId,
            Nombre = dto.Nombre,
            Nivel = dto.Nivel,
            Descripcion = dto.Descripcion,
            Activo = true
        };

        _db.Roles.Add(rol);
        await _db.SaveChangesAsync();

        return new RolDto(rol.Id, rol.Nombre, rol.Nivel, rol.Descripcion, rol.Activo);
    }

    public async Task EliminarRolAsync(int proyectoId, int rolId)
    {
        await ValidarProyectoExisteAsync(proyectoId);

        var rol = await _db.Roles.FirstOrDefaultAsync(r => r.Id == rolId && r.ProyectoId == proyectoId)
            ?? throw new KeyNotFoundException("Rol no encontrado en el proyecto.");

        if (await _db.ProyectoUsuarioRoles.AnyAsync(pur => pur.RolId == rolId && pur.Activo))
            throw new InvalidOperationException("No se puede eliminar: hay usuarios asignados a este rol.");

        _db.Roles.Remove(rol);
        await _db.SaveChangesAsync();
    }

    // ── Vistas ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<VistaDto>> ObtenerVistasAsync(int proyectoId)
    {
        await ValidarProyectoExisteAsync(proyectoId);

        return await _db.Vistas
            .Where(v => v.ProyectoId == proyectoId && v.Activo)
            .OrderBy(v => v.VistaPadreId)   // raíces primero, luego hijos
            .ThenBy(v => v.Orden)
            .ThenBy(v => v.Nombre)
            .Select(v => new VistaDto(
                v.Id, v.Codigo, v.Nombre, v.Ruta, v.Icono,
                v.Descripcion, v.Orden, v.Activo,
                v.VistaPadreId, v.EsContenedor))   // ← nuevos campos
            .ToListAsync();
    }

    public async Task<VistaDto> CrearVistaAsync(int proyectoId, CrearVistaDto dto)
    {
        await ValidarProyectoExisteAsync(proyectoId);

        if (await _db.Vistas.AnyAsync(v => v.ProyectoId == proyectoId && v.Codigo == dto.Codigo))
            throw new InvalidOperationException($"Ya existe una vista con el código '{dto.Codigo}'.");

        // Validar que el padre (si se especificó) exista en el mismo proyecto
        if (dto.VistaPadreId.HasValue)
        {
            var padreExiste = await _db.Vistas.AnyAsync(v =>
                v.Id == dto.VistaPadreId.Value && v.ProyectoId == proyectoId);

            if (!padreExiste)
                throw new InvalidOperationException("La vista padre especificada no existe en este proyecto.");
        }

        var vista = new Vista
        {
            ProyectoId = proyectoId,
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Ruta = dto.Ruta,
            Icono = dto.Icono,
            Descripcion = dto.Descripcion,
            Orden = dto.Orden,
            VistaPadreId = dto.VistaPadreId,
            EsContenedor = dto.EsContenedor,
            Activo = true
        };

        _db.Vistas.Add(vista);
        await _db.SaveChangesAsync();

        return new VistaDto(
            vista.Id, vista.Codigo, vista.Nombre, vista.Ruta, vista.Icono,
            vista.Descripcion, vista.Orden, vista.Activo,
            vista.VistaPadreId, vista.EsContenedor);
    }

    public async Task EliminarVistaAsync(int proyectoId, int vistaId)
    {
        await ValidarProyectoExisteAsync(proyectoId);

        var vista = await _db.Vistas.FirstOrDefaultAsync(v => v.Id == vistaId && v.ProyectoId == proyectoId)
            ?? throw new KeyNotFoundException("Vista no encontrada en el proyecto.");

        // No borrar si tiene hijos (FK auto-referencial NoAction)
        var tieneHijos = await _db.Vistas.AnyAsync(v => v.VistaPadreId == vistaId);
        if (tieneHijos)
            throw new InvalidOperationException("No se puede eliminar: la vista tiene sub-vistas asociadas. Elimínalas primero.");

        // No borrar si hay accesos de usuario apuntando a ella
        if (await _db.UsuarioVistasAcceso.AnyAsync(uva => uva.VistaId == vistaId))
            throw new InvalidOperationException("No se puede eliminar: hay accesos de usuario asociados a esta vista.");

        _db.Vistas.Remove(vista);
        await _db.SaveChangesAsync();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task ValidarProyectoExisteAsync(int proyectoId)
    {
        if (!await _db.Proyectos.AnyAsync(p => p.Id == proyectoId))
            throw new KeyNotFoundException($"Proyecto con Id {proyectoId} no encontrado.");
    }

    private static void ValidarPlataforma(string plataforma)
    {
        string[] validas = ["Web", "Desktop", "Mobile", "Web+Mobile", "Web+Desktop", "Todos"];
        if (!validas.Contains(plataforma))
            throw new InvalidOperationException($"Plataforma '{plataforma}' no válida.");
    }

    private static void ValidarEstado(string estado)
    {
        string[] validos = ["Produccion", "Mantenimiento", "Desarrollo"];
        if (!validos.Contains(estado))
            throw new InvalidOperationException($"Estado '{estado}' no válido.");
    }

    public async Task<ProyectoDetalleDto?> ObtenerPorCodigoAsync(string codigo)
    {
        var proyecto = await _db.Proyectos
            .Include(p => p.Roles)
            .Include(p => p.Vistas)
            .FirstOrDefaultAsync(p => p.Codigo == codigo);

        return proyecto is null ? null : MapProyectoDetalle(proyecto);
    }

    private static ProyectoDetalleDto MapProyectoDetalle(Proyecto proyecto)
    {
        return new ProyectoDetalleDto(
            proyecto.Id, proyecto.Codigo, proyecto.Nombre, proyecto.Plataforma,
            proyecto.IconoCss, proyecto.UrlBase, proyecto.Estado, proyecto.Version,
            proyecto.Descripcion, proyecto.Orden, proyecto.Activo,
            proyecto.Roles.Select(r => new RolDto(r.Id, r.Nombre, r.Nivel, r.Descripcion, r.Activo)),
            proyecto.Vistas.Select(v => new VistaDto(
                v.Id, v.Codigo, v.Nombre, v.Ruta, v.Icono,
                v.Descripcion, v.Orden, v.Activo,
                v.VistaPadreId, v.EsContenedor))
        );
    }
}