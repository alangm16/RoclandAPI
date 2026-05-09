using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.Usuarios;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class UsuarioService : IUsuarioService
{
    private readonly SuperAdminDbContext _db;

    public UsuarioService(SuperAdminDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<UsuarioListDto>> ObtenerTodosAsync()
    {
        return await _db.Usuarios
            .Include(u => u.RolSA)
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new UsuarioListDto(
                u.Id,
                u.NombreCompleto,
                u.Username,
                u.Email,
                u.RolSA != null ? u.RolSA.Nombre : null,
                u.Activo,
                u.FechaCreacion
            ))
            .ToListAsync();
    }

    public async Task<UsuarioDetalleDto?> ObtenerPorIdAsync(int id)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.RolSA)
            .Include(u => u.ProyectosAsignados)
                .ThenInclude(pur => pur.Proyecto)
            .Include(u => u.ProyectosAsignados)
                .ThenInclude(pur => pur.Rol)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario is null) return null;

        return MapUsuarioDetalle(usuario);
    }

    public async Task<UsuarioDetalleDto> CrearAsync(CrearUsuarioDto dto)
    {
        // Validar unicidad del username
        if (await _db.Usuarios.AnyAsync(u => u.Username == dto.Username))
            throw new InvalidOperationException($"El username '{dto.Username}' ya está en uso.");

        // Validar que el RolSA exista si se proporciona
        if (dto.RolSAId.HasValue)
        {
            var rolSAExiste = await _db.RolesSA.AnyAsync(r => r.Id == dto.RolSAId.Value && r.Activo);
            if (!rolSAExiste)
                throw new InvalidOperationException("El RolSA especificado no existe o está inactivo.");
        }

        var usuario = new Usuario
        {
            NombreCompleto = dto.NombreCompleto,
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RolSAId = dto.RolSAId,
            Activo = true
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        // Recargar para incluir navegaciones
        await _db.Entry(usuario).Reference(u => u.RolSA).LoadAsync();
        return MapUsuarioDetalle(usuario);
    }

    public async Task<UsuarioDetalleDto> ActualizarAsync(int id, ActualizarUsuarioDto dto)
    {
        var usuario = await _db.Usuarios.FindAsync(id)
            ?? throw new KeyNotFoundException($"Usuario con Id {id} no encontrado.");

        // Validar RolSA si se cambia
        if (dto.RolSAId.HasValue)
        {
            var rolSAExiste = await _db.RolesSA.AnyAsync(r => r.Id == dto.RolSAId.Value && r.Activo);
            if (!rolSAExiste)
                throw new InvalidOperationException("El RolSA especificado no existe o está inactivo.");
        }

        usuario.NombreCompleto = dto.NombreCompleto;
        usuario.Email = dto.Email;
        usuario.RolSAId = dto.RolSAId;

        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync();

        // Recargar datos completos
        await _db.Entry(usuario).Reference(u => u.RolSA).LoadAsync();
        await _db.Entry(usuario).Collection(u => u.ProyectosAsignados).LoadAsync();
        foreach (var pur in usuario.ProyectosAsignados)
        {
            await _db.Entry(pur).Reference(p => p.Proyecto).LoadAsync();
            await _db.Entry(pur).Reference(p => p.Rol).LoadAsync();
        }

        return MapUsuarioDetalle(usuario);
    }

    public async Task DesactivarAsync(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id)
            ?? throw new KeyNotFoundException($"Usuario con Id {id} no encontrado.");

        usuario.Activo = false;
        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync();
    }

    public async Task AsignarProyectoRolAsync(int usuarioId, AsignarProyectoRolDto dto)
    {
        // Validar existencia
        var usuarioExiste = await _db.Usuarios.AnyAsync(u => u.Id == usuarioId);
        if (!usuarioExiste)
            throw new KeyNotFoundException("Usuario no encontrado.");

        var proyectoExiste = await _db.Proyectos.AnyAsync(p => p.Id == dto.ProyectoId);
        if (!proyectoExiste)
            throw new KeyNotFoundException("Proyecto no encontrado.");

        var rolExiste = await _db.Roles.AnyAsync(r => r.Id == dto.RolId && r.ProyectoId == dto.ProyectoId);
        if (!rolExiste)
            throw new InvalidOperationException("El rol no pertenece al proyecto especificado.");

        // Buscar asignación existente (puede estar inactiva)
        var asignacion = await _db.ProyectoUsuarioRoles
            .FirstOrDefaultAsync(pur =>
                pur.UsuarioId == usuarioId &&
                pur.ProyectoId == dto.ProyectoId);

        if (asignacion != null)
        {
            // Reactivar si estaba inactiva y actualizar rol
            asignacion.RolId = dto.RolId;
            asignacion.Activo = true;
            _db.ProyectoUsuarioRoles.Update(asignacion);
        }
        else
        {
            asignacion = new ProyectoUsuarioRol
            {
                UsuarioId = usuarioId,
                ProyectoId = dto.ProyectoId,
                RolId = dto.RolId,
                Activo = true
            };
            _db.ProyectoUsuarioRoles.Add(asignacion);
        }

        await _db.SaveChangesAsync();
    }

    public async Task RevocarProyectoAsync(int usuarioId, int proyectoId)
    {
        var asignacion = await _db.ProyectoUsuarioRoles
            .FirstOrDefaultAsync(pur =>
                pur.UsuarioId == usuarioId &&
                pur.ProyectoId == proyectoId &&
                pur.Activo)
            ?? throw new InvalidOperationException("El usuario no tiene una asignación activa para este proyecto.");

        asignacion.Activo = false;
        _db.ProyectoUsuarioRoles.Update(asignacion);
        await _db.SaveChangesAsync();
    }

    public async Task ActualizarVistasAccesoAsync(int usuarioId, int proyectoId, IEnumerable<int> vistaIds)
    {
        // Validar que el usuario tenga acceso al proyecto (opcional, pero seguro)
        var tieneAcceso = await _db.ProyectoUsuarioRoles
            .AnyAsync(pur => pur.UsuarioId == usuarioId
                          && pur.ProyectoId == proyectoId
                          && pur.Activo);
        if (!tieneAcceso)
            throw new InvalidOperationException("El usuario no tiene acceso activo a este proyecto.");

        // Validar que todos los vistaIds pertenezcan al proyecto
        var vistaIdsDistintos = vistaIds.Distinct().ToList();
        var vistasValidas = await _db.Vistas
            .Where(v => v.ProyectoId == proyectoId && vistaIdsDistintos.Contains(v.Id))
            .Select(v => v.Id)
            .ToListAsync();

        if (vistasValidas.Count != vistaIdsDistintos.Count)
            throw new InvalidOperationException("Algunas vistas no pertenecen al proyecto.");

        // Remover todos los accesos actuales del usuario en el proyecto
        var accesosActuales = await _db.UsuarioVistasAcceso
            .Where(uva => uva.UsuarioId == usuarioId
                       && uva.ProyectoId == proyectoId)
            .ToListAsync();

        _db.UsuarioVistasAcceso.RemoveRange(accesosActuales);

        // Insertar los nuevos accesos
        foreach (var vistaId in vistaIds)
        {
            _db.UsuarioVistasAcceso.Add(new UsuarioVistaAcceso
            {
                UsuarioId = usuarioId,
                ProyectoId = proyectoId,
                VistaId = vistaId,
                TieneAcceso = true
            });
        }

        await _db.SaveChangesAsync();
    }

    private static UsuarioDetalleDto MapUsuarioDetalle(Usuario usuario)
    {
        return new UsuarioDetalleDto(
            usuario.Id,
            usuario.NombreCompleto,
            usuario.Username,
            usuario.Email,
            usuario.QRCode,
            usuario.RolSA?.Nombre,
            usuario.Activo,
            usuario.UltimoAcceso ?? DateTime.MinValue,
            usuario.ProyectosAsignados?.Select(pur => new ProyectoAsignadoDto(
                pur.ProyectoId,
                pur.Proyecto?.Codigo ?? "",
                pur.Proyecto?.Nombre ?? "",
                pur.Rol?.Nombre ?? "",
                pur.Rol?.Nivel ?? 0,
                pur.Activo
            )) ?? Enumerable.Empty<ProyectoAsignadoDto>()
        );
    }
}