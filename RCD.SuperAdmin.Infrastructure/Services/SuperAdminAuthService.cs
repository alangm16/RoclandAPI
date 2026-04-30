
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using RCD.Shared.Infrastructure.Security;
using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Infrastructure.Data;
using RCD.SuperAdmin.Application.Interfaces;

namespace RCD.SuperAdmin.Infrastructure.Services
{
    public class SuperAdminAuthService(SuperAdminDbContext db, IJwtTokenService jwtService) : ISuperAdminAuthService
    {
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var usuario = await db.Usuarios
                .Include(u => u.Roles).ThenInclude(ur => ur.Rol)
                .Include(u => u.Permisos).ThenInclude(p => p.Proyecto)
                .Include(u => u.Permisos).ThenInclude(p => p.Vista)
                .FirstOrDefaultAsync(u => u.UserName == request.Username && u.Activo);

            if (usuario is null) return null;

            // validar password con BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
                return null;

            var roles = usuario.Roles.Select(ur => ur.Rol.Nombre).ToList();
            var token = jwtService.GenerarToken(usuario.Id, usuario.UserName, roles);

            var proyectosPermitidos = await ObtenerProyectosPermitidosInternoAsync(usuario.Id);

            return new LoginResponse(token, usuario.NombreCompleto, usuario.UserName, roles, proyectosPermitidos);
        }

        private async Task<IEnumerable<ProyectoPermitidoDto>> ObtenerProyectosPermitidosInternoAsync(int usuarioId)
        {
            // Trae todos los permisos del usuario agrupados por proyecto
            var permisos = await db.PermisoUsuarios
                .Where(p => p.UsuarioId == usuarioId && p.Proyecto.Activo)
                .Include(p => p.Proyecto)
                .Include(p => p.Vista)
                .ToListAsync();

            return permisos
                .GroupBy(p => p.Proyecto)
                .Select(g => new ProyectoPermitidoDto(
                    g.Key.Id,
                    g.Key.Codigo,
                    g.Key.Nombre,
                    g.Key.Plataforma,
                    g.Key.UrlBase,
                    g.Key.IconoCss,
                    g.Where(p => p.Vista is not null)
                     .Select(p => new VistaPermitidaDto(
                         p.Vista!.Id, p.Vista.Codigo, p.Vista.Nombre, p.Vista.Icono))
                ));
        }
    }
}
