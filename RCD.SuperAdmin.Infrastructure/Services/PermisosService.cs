// PermisosService.cs
using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.Auth;
using RCD.SuperAdmin.Application.DTOs.Permisos;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class PermisosService(SuperAdminDbContext db) : IPermisosService
{
    // ---------------------------------------------------------------
    // RESOLUCIÓN EFECTIVA: Rol como base + Usuario como override
    // ---------------------------------------------------------------
    public async Task<IEnumerable<ProyectoPermitidoDto>> ResolverPermisosEfectivosAsync(int usuarioId)
    {
        var rolIds = await db.UsuarioRoles
            .Where(ur => ur.UsuarioId == usuarioId)
            .Select(ur => ur.RolId)
            .ToListAsync();

        var permisosRol = await db.PermisosRol
            .Where(p => rolIds.Contains(p.RolId) && p.Proyecto.Activo)
            .Include(p => p.Proyecto)
            .Include(p => p.Vista)
            .ToListAsync();

        var permisosUsuario = await db.PermisosUsuario
            .Where(p => p.UsuarioId == usuarioId && p.Proyecto.Activo)
            .Include(p => p.Proyecto)
            .Include(p => p.Vista)
            .ToListAsync();

        var proyectoIds = permisosRol.Select(p => p.ProyectoId)
            .Union(permisosUsuario.Select(p => p.ProyectoId))
            .Distinct();

        var proyectos = await db.Proyectos
            .Where(p => proyectoIds.Contains(p.Id) && p.Activo)
            .Include(p => p.Vistas.Where(v => v.Activo))
            .OrderBy(p => p.Orden)
            .ToListAsync();

        var resultado = new List<ProyectoPermitidoDto>();

        foreach (var pr in proyectos)
        {
            var permisoUsuarioPr = permisosUsuario
                .FirstOrDefault(p => p.ProyectoId == pr.Id && p.VistaId is null);

            var permisoRolPr = permisosRol
                .Where(p => p.ProyectoId == pr.Id && p.VistaId is null)
                .Aggregate((PermisoRol?)null, AgregarPermisosRol);

            var permisoPr = permisoUsuarioPr is not null
                ? MapearDesdeUsuario(permisoUsuarioPr)
                : permisoRolPr is not null
                    ? MapearDesdeRol(permisoRolPr)
                    : null;

            if (permisoPr is null &&
                !permisosUsuario.Any(p => p.ProyectoId == pr.Id) &&
                !permisosRol.Any(p => p.ProyectoId == pr.Id))
                continue;

            var vistas = new List<VistaPermitidaDto>();

            foreach (var v in pr.Vistas)
            {
                var puVista = permisosUsuario.FirstOrDefault(p => p.VistaId == v.Id);
                var prVista = permisosRol
                    .Where(p => p.VistaId == v.Id)
                    .Aggregate((PermisoRol?)null, AgregarPermisosRol);

                var permisoVista = puVista is not null
                    ? MapearDesdeUsuario(puVista)
                    : prVista is not null
                        ? MapearDesdeRol(prVista)
                        : null;

                if (permisoVista is null) continue;

                vistas.Add(new VistaPermitidaDto(
                    v.Id, v.Codigo, v.Nombre, v.Icono,
                    permisoVista.PuedeLeer, permisoVista.PuedeCrear,
                    permisoVista.PuedeEditar, permisoVista.PuedeBorrar));
            }

            resultado.Add(new ProyectoPermitidoDto(
                pr.Id, pr.Codigo, pr.Nombre, pr.Plataforma,
                pr.UrlBase, pr.IconoCss,
                AccesoTotal: permisoPr is not null,
                PuedeLeer: permisoPr?.PuedeLeer ?? false,
                PuedeCrear: permisoPr?.PuedeCrear ?? false,
                PuedeEditar: permisoPr?.PuedeEditar ?? false,
                PuedeBorrar: permisoPr?.PuedeBorrar ?? false,
                vistas));
        }

        return resultado;
    }

    // ---------------------------------------------------------------
    // MATRICES para el Super Panel
    // ---------------------------------------------------------------
    public async Task<MatrizPermisosDto> ObtenerMatrizRolAsync(int rolId)
    {
        var rol = await db.Roles.FindAsync(rolId)
                  ?? throw new KeyNotFoundException("Rol no encontrado");

        var proyectos = await db.Proyectos
            .Where(p => p.Activo)
            .Include(p => p.Vistas.Where(v => v.Activo))
            .OrderBy(p => p.Orden)
            .ToListAsync();

        var permisos = await db.PermisosRol
            .Where(p => p.RolId == rolId)
            .ToListAsync();

        return ConstruirMatriz(rolId, rol.Nombre, "Rol", proyectos, permisos, []);
    }

    public async Task<MatrizPermisosDto> ObtenerMatrizUsuarioAsync(int usuarioId)
    {
        var usuario = await db.Usuarios.FindAsync(usuarioId)
                      ?? throw new KeyNotFoundException("Usuario no encontrado");

        var rolIds = await db.UsuarioRoles
            .Where(ur => ur.UsuarioId == usuarioId)
            .Select(ur => ur.RolId)
            .ToListAsync();

        var proyectos = await db.Proyectos
            .Where(p => p.Activo)
            .Include(p => p.Vistas.Where(v => v.Activo))
            .OrderBy(p => p.Orden)
            .ToListAsync();

        var permisosRol = await db.PermisosRol
            .Where(p => rolIds.Contains(p.RolId))
            .ToListAsync();

        var permisosUsuario = await db.PermisosUsuario
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();

        return ConstruirMatriz(usuarioId, usuario.NombreCompleto, "Usuario",
                               proyectos, permisosRol, permisosUsuario);
    }

    // ---------------------------------------------------------------
    // UPSERT
    // ---------------------------------------------------------------
    public async Task UpsertPermisoRolAsync(AsignarPermisoRolRequest req)
    {
        var existente = await db.PermisosRol.FirstOrDefaultAsync(p =>
            p.RolId == req.RolId &&
            p.ProyectoId == req.ProyectoId &&
            p.VistaId == req.VistaId);

        if (existente is not null)
        {
            existente.PuedeLeer = req.PuedeLeer;
            existente.PuedeCrear = req.PuedeCrear;
            existente.PuedeEditar = req.PuedeEditar;
            existente.PuedeBorrar = req.PuedeBorrar;
        }
        else
        {
            db.PermisosRol.Add(new PermisoRol
            {
                RolId = req.RolId,
                ProyectoId = req.ProyectoId,
                VistaId = req.VistaId,
                PuedeLeer = req.PuedeLeer,
                PuedeCrear = req.PuedeCrear,
                PuedeEditar = req.PuedeEditar,
                PuedeBorrar = req.PuedeBorrar
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task UpsertPermisoUsuarioAsync(AsignarPermisoUsuarioRequest req)
    {
        var existente = await db.PermisosUsuario.FirstOrDefaultAsync(p =>
            p.UsuarioId == req.UsuarioId &&
            p.ProyectoId == req.ProyectoId &&
            p.VistaId == req.VistaId);

        if (existente is not null)
        {
            existente.PuedeLeer = req.PuedeLeer;
            existente.PuedeCrear = req.PuedeCrear;
            existente.PuedeEditar = req.PuedeEditar;
            existente.PuedeBorrar = req.PuedeBorrar;
        }
        else
        {
            db.PermisosUsuario.Add(new PermisoUsuario
            {
                UsuarioId = req.UsuarioId,
                ProyectoId = req.ProyectoId,
                VistaId = req.VistaId,
                PuedeLeer = req.PuedeLeer,
                PuedeCrear = req.PuedeCrear,
                PuedeEditar = req.PuedeEditar,
                PuedeBorrar = req.PuedeBorrar
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task RevocarPermisoRolAsync(int rolId, int proyectoId, int? vistaId)
    {
        var p = await db.PermisosRol.FirstOrDefaultAsync(x =>
            x.RolId == rolId && x.ProyectoId == proyectoId && x.VistaId == vistaId);
        if (p is not null) { db.PermisosRol.Remove(p); await db.SaveChangesAsync(); }
    }

    public async Task RevocarPermisoUsuarioAsync(int usuarioId, int proyectoId, int? vistaId)
    {
        var p = await db.PermisosUsuario.FirstOrDefaultAsync(x =>
            x.UsuarioId == usuarioId && x.ProyectoId == proyectoId && x.VistaId == vistaId);
        if (p is not null) { db.PermisosUsuario.Remove(p); await db.SaveChangesAsync(); }
    }

    // ---------------------------------------------------------------
    // Helpers privados
    // ---------------------------------------------------------------
    private static MatrizPermisosDto ConstruirMatriz(
        int entidadId, string nombre, string tipo,
        List<Proyecto> proyectos,
        IEnumerable<PermisoRol> permisosRol,
        IEnumerable<PermisoUsuario> permisosUsuario)
    {
        var filas = proyectos.Select(pr =>
        {
            var pRolPr = permisosRol.FirstOrDefault(p =>
                p.ProyectoId == pr.Id && p.VistaId is null);
            var pUsuPr = permisosUsuario.FirstOrDefault(p =>
                p.ProyectoId == pr.Id && p.VistaId is null);

            PermisoCrudDto? permisoPr = pUsuPr is not null
                ? new(pUsuPr.PuedeLeer, pUsuPr.PuedeCrear, pUsuPr.PuedeEditar, pUsuPr.PuedeBorrar)
                : pRolPr is not null
                    ? new(pRolPr.PuedeLeer, pRolPr.PuedeCrear, pRolPr.PuedeEditar, pRolPr.PuedeBorrar)
                    : null;

            var vistas = pr.Vistas.Select(v =>
            {
                var pRolV = permisosRol.FirstOrDefault(p => p.VistaId == v.Id);
                var pUsuV = permisosUsuario.FirstOrDefault(p => p.VistaId == v.Id);

                PermisoCrudDto? permisoV = pUsuV is not null
                    ? new(pUsuV.PuedeLeer, pUsuV.PuedeCrear, pUsuV.PuedeEditar, pUsuV.PuedeBorrar)
                    : pRolV is not null
                        ? new(pRolV.PuedeLeer, pRolV.PuedeCrear, pRolV.PuedeEditar, pRolV.PuedeBorrar)
                        : null;

                return new VistaMatrizDto(v.Id, v.Codigo, v.Nombre, v.Icono, permisoV);
            });

            return new ProyectoMatrizDto(pr.Id, pr.Codigo, pr.Nombre, permisoPr, vistas);
        });

        return new MatrizPermisosDto(entidadId, nombre, tipo, filas);
    }

    private record PermisoResuelto(
        bool PuedeLeer,
        bool PuedeCrear,
        bool PuedeEditar,
        bool PuedeBorrar
    );

    private static PermisoResuelto MapearDesdeUsuario(PermisoUsuario p) =>
        new(p.PuedeLeer, p.PuedeCrear, p.PuedeEditar, p.PuedeBorrar);

    private static PermisoResuelto MapearDesdeRol(PermisoRol p) =>
        new(p.PuedeLeer, p.PuedeCrear, p.PuedeEditar, p.PuedeBorrar);

    private static PermisoRol? AgregarPermisosRol(PermisoRol? acum, PermisoRol siguiente)
    {
        if (acum is null) return siguiente;
        acum.PuedeLeer |= siguiente.PuedeLeer;
        acum.PuedeCrear |= siguiente.PuedeCrear;
        acum.PuedeEditar |= siguiente.PuedeEditar;
        acum.PuedeBorrar |= siguiente.PuedeBorrar;
        return acum;
    }
}