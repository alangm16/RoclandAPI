

using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services
{
    public class PermisosService(SuperAdminDbContext db) : IPermisosService
    {
        public async Task<MatrizPermisosDto> ObtenerMatrizPermisosAsync(int usuarioId)
        {
            var usuario = await db.Usuarios.FindAsync(usuarioId)
                          ?? throw new KeyNotFoundException("Usuario no encontrado");

            var proyectos = await db.Proyectos
                .Where(p => p.Activo)
                .Include(p => p.Vistas.Where(v => v.Activo))
                .OrderBy(p => p.Orden)
                .ToListAsync();

            var permisosUsuario = await db.PermisoUsuarios
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();

            var matriz = proyectos.Select(pr => new ProyectoMatrizDto(
                pr.Id,
                pr.Codigo,
                pr.Nombre,
                TieneAccesoTotal: permisosUsuario.Any(p => p.ProyectoId == pr.Id && p.VistaId is null),
                Vistas: pr.Vistas.Select(v => new VistaMatrizDto(
                    v.Id,
                    v.Codigo,
                    v.Nombre,
                    TieneAcceso: permisosUsuario.Any(p => p.VistaId == v.Id)
                ))
            ));

            return new MatrizPermisosDto(usuarioId, usuario.NombreCompleto, matriz);
        }

        public async Task AsignarPermisoAsync(AsignarPermisoRequest request)
        {
            var yaExiste = await db.PermisoUsuarios.AnyAsync(p =>
                p.UsuarioId == request.UsuarioId &&
                p.ProyectoId == request.ProyectoId &&
                p.VistaId == request.VistaId);

            if (yaExiste) return;

            db.PermisoUsuarios.Add(new Domain.Entities.PermisoUsuario
            {
                UsuarioId = request.UsuarioId,
                ProyectoId = request.ProyectoId,
                VistaId = request.VistaId
            });
            await db.SaveChangesAsync();
        }

        public async Task RevocarPermisoAsync(int permisoId)
        {
            var permiso = await db.PermisoUsuarios.FindAsync(permisoId)
                          ?? throw new KeyNotFoundException("Permiso no encontrado");
            db.PermisoUsuarios.Remove(permiso);
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProyectoPermitidoDto>> ObtenerProyectosPermitidosAsync(int usuarioId)
        {
            // Reutilizado para endpoints de consulta desde el Super Panel
            var permisos = await db.PermisoUsuarios
                .Where(p => p.UsuarioId == usuarioId && p.Proyecto.Activo)
                .Include(p => p.Proyecto)
                .Include(p => p.Vista)
                .ToListAsync();

            return permisos.GroupBy(p => p.Proyecto)
                .Select(g => new ProyectoPermitidoDto(
                    g.Key.Id, g.Key.Codigo, g.Key.Nombre,
                    g.Key.Plataforma, g.Key.UrlBase, g.Key.IconoCss,
                    g.Where(p => p.Vista is not null)
                     .Select(p => new VistaPermitidaDto(
                         p.Vista!.Id, p.Vista.Codigo, p.Vista.Nombre, p.Vista.Icono))));
        }
    }
}
