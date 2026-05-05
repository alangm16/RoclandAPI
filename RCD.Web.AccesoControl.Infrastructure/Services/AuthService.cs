using Microsoft.EntityFrameworkCore;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Application.Interfaces;
using RCD.Web.AccesoControl.Infrastructure.Persistence;

namespace RCD.Web.AccesoControl.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AccesoControlWebDbContext _context;

        public AuthService(AccesoControlWebDbContext context)
        {
            _context = context;
        }

        public async Task<PerfilDto?> ObtenerPerfilPorSuperAdminIdAsync(int superAdminUsuarioId)
        {
            var perfil = await _context.Perfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.SuperAdminUsuarioId == superAdminUsuarioId && p.Activo);

            if (perfil == null)
            {
                return null;
            }

            return new PerfilDto
            {
                Id = perfil.Id,
                SuperAdminUsuarioId = perfil.SuperAdminUsuarioId,
                NombreCompleto = perfil.NombreCompleto,
                NumeroEmpleado = perfil.NumeroEmpleado,
                TipoPerfil = perfil.TipoPerfil,
                Turno = perfil.Turno,
                Activo = perfil.Activo
            };
        }

        public async Task<bool> TienePermisoAsync(int superAdminUsuarioId, string tipoPerfilRequerido)
        {
            var perfil = await _context.Perfiles
               .AsNoTracking()
               .FirstOrDefaultAsync(p => p.SuperAdminUsuarioId == superAdminUsuarioId && p.Activo);

            if (perfil == null) return false;

            // Lógica simple: Si requiere "Guardia", el perfil debe ser "Guardia" o superior (ej: Administrador).
            // Adapta esto según tus necesidades exactas.
            if (tipoPerfilRequerido == "Administrador" && perfil.TipoPerfil != "Administrador" && perfil.TipoPerfil != "Gerente")
            {
                return false;
            }

            return true;
        }
    }
}