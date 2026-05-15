using Microsoft.EntityFrameworkCore;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Domain.Models.Entities;
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
            // 1. Buscar el perfil local
            var perfil = await _context.Perfiles
                .FirstOrDefaultAsync(p => p.SuperAdminUsuarioId == superAdminUsuarioId);

            // 2. Validar usuario en SuperAdmin (activo y existe)
            //    Consulta SQL directa para evitar entidades SA en el contexto.
            var usuarioSA = await _context.Database
                .SqlQueryRaw<UsuarioSuperAdminDto>(
                    @"SELECT Id, NombreCompleto, Activo
                      FROM TBL_ROCLAND_SUPERADMIN_USUARIOS
                      WHERE Id = {0}",
                    superAdminUsuarioId)
                .FirstOrDefaultAsync();

            if (usuarioSA == null || usuarioSA.Activo == false)
                return null; // Usuario no existe o está inactivo

            // 3. Verificar que el usuario tenga asignación activa al proyecto 'acceso-control'
            //    con rol 'Guardia'
            const string sqlAsignacion = @"
                SELECT COUNT(1) AS Value
                FROM TBL_ROCLAND_SUPERADMIN_PROYECTO_USUARIO_ROL pur
                INNER JOIN TBL_ROCLAND_SUPERADMIN_PROYECTOS p ON pur.ProyectoId = p.Id
                INNER JOIN TBL_ROCLAND_SUPERADMIN_ROLES r ON pur.RolId = r.Id
                WHERE pur.UsuarioId = {0}
                  AND p.Codigo = 'acceso-control'
                  AND r.Nombre = 'Guardia'
                  AND pur.Activo = 1
                  AND p.Activo = 1
                  AND r.Activo = 1";

                        // Cambiamos ExecuteSqlRawAsync por SqlQueryRaw
                        var conteo = await _context.Database
                            .SqlQueryRaw<int>(sqlAsignacion, superAdminUsuarioId)
                            .FirstOrDefaultAsync();

                        var esGuardia = conteo > 0;

                        if (!esGuardia)
                            return null;

            // 4. Si el perfil local ya existe
            if (perfil != null)
            {
                // 4a. Si el perfil local está inactivo, denegar acceso
                if (!perfil.Activo)
                    return null;

                // 4b. Sincronizar NombreCompleto si cambió en SuperAdmin
                if (perfil.NombreCompleto != usuarioSA.NombreCompleto)
                {
                    perfil.NombreCompleto = usuarioSA.NombreCompleto;
                    perfil.FechaModificacion = DateTime.UtcNow;
                    _context.Perfiles.Update(perfil);
                    await _context.SaveChangesAsync();
                }

                return new PerfilContextoDto(
                    PerfilId: perfil.Id,
                    SuperAdminUsuarioId: perfil.SuperAdminUsuarioId,
                    NombreCompleto: perfil.NombreCompleto,
                    NombreRol: string.Empty,   // el controller lo llena desde el JWT
                    NivelRol: 0,
                    Turno: perfil.Turno,
                    NumeroEmpleado: perfil.NumeroEmpleado
                );
            }

            // 5. No existe perfil local → Crearlo automáticamente
            var nuevoPerfil = new Perfil
            {
                SuperAdminUsuarioId = superAdminUsuarioId,
                NombreCompleto = usuarioSA.NombreCompleto,
                NumeroEmpleado = null,
                Turno = null,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                CreadoPor = null
            };

            _context.Perfiles.Add(nuevoPerfil);
            await _context.SaveChangesAsync();

            return new PerfilContextoDto(
                PerfilId: nuevoPerfil.Id,
                SuperAdminUsuarioId: nuevoPerfil.SuperAdminUsuarioId,
                NombreCompleto: nuevoPerfil.NombreCompleto,
                NombreRol: string.Empty,
                NivelRol: 0,
                Turno: null,
                NumeroEmpleado: null
            );
        }

        // DTO privado para el resultado de la consulta SQL
        private class UsuarioSuperAdminDto
        {
            public int Id { get; set; }
            public string NombreCompleto { get; set; } = string.Empty;
            public bool Activo { get; set; }
        }
    }
}