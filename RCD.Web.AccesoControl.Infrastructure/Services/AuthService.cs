using Microsoft.EntityFrameworkCore;
using RCD.AccesoControlWeb.Application.DTOs;
using RCD.Web.AccesoControl.Infrastructure.Data;

namespace RCD.Web.AccesoControl.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AccesoControlWebDbContext _context;

        public AuthService(AccesoControlWebDbContext context)
        {
            _context = context;
        }

        public async Task<PerfilContextoDto?> ObtenerPerfilContextoAsync(int superAdminUsuarioId)
        {
            var perfil = await _context.Perfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.SuperAdminUsuarioId == superAdminUsuarioId && p.Activo);

            if (perfil is null) return null;

            return new PerfilContextoDto(
                PerfilId: perfil.Id,
                SuperAdminUsuarioId: perfil.SuperAdminUsuarioId,
                NombreCompleto: perfil.NombreCompleto,
                NombreRol: string.Empty,  // se completa desde el JWT en el controller
                NivelRol: 0,             // se completa desde el JWT en el controller
                Turno: perfil.Turno,
                NumeroEmpleado: perfil.NumeroEmpleado
            );
        }
    }
}