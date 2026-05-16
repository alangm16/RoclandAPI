using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs;
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

    public async Task<PagedResult<UsuarioListDto>> ObtenerTodosAsync(bool soloPanel = false, int pagina = 1, int tamanoPagina = 20, bool? activo = null)
    {
        var query = _db.Usuarios.AsQueryable();  // ← sin Include

        if (activo.HasValue)
            query = query.Where(u => u.Activo == activo.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(u => u.NombreCompleto)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(u => new UsuarioListDto(
                u.Id,
                u.NombreCompleto,
                u.Username,
                u.Email,
                u.Activo,
                u.FechaCreacion,
                u.UltimoAcceso,
                u.BloqueadoHasta
            ))
            .ToListAsync();

        return new PagedResult<UsuarioListDto>(items, total, pagina, tamanoPagina);
    }

    public async Task<UsuarioDetalleDto?> ObtenerPorIdAsync(int id)
    {
        // Proyección directa con datos de auditoría
        var detalle = await _db.Usuarios
            .Where(u => u.Id == id)
            .Select(u => new UsuarioDetalleDto(
                u.Id,
                u.NombreCompleto,
                u.Username,
                u.Email,
                u.QRCode,
                u.Activo,
                u.UltimoAcceso,
                u.ProyectosAsignados
                    .Where(pur => pur.Activo)
                    .Select(pur => new ProyectoAsignadoDto(
                        pur.ProyectoId,
                        pur.Proyecto.Codigo,
                        pur.Proyecto.Nombre,
                        pur.Rol.Nombre,
                        pur.Rol.Nivel,
                        pur.Activo
                    )).ToList(),
                u.IntentosFallidos,
                u.BloqueadoHasta,
                _db.Usuarios.Where(c => c.Id == u.CreadoPor).Select(c => c.Username).FirstOrDefault(),
                u.FechaCreacion,
                _db.Usuarios.Where(c => c.Id == u.ModificadoPor).Select(c => c.Username).FirstOrDefault(),
                u.FechaModificacion
            ))
            .FirstOrDefaultAsync();

        return detalle;
    }

    public async Task<UsuarioDetalleDto> CrearAsync(CrearUsuarioDto dto)
    {
        // Validar unicidad del username
        if (await _db.Usuarios.AnyAsync(u => u.Username == dto.Username))
            throw new InvalidOperationException($"El username '{dto.Username}' ya está en uso.");

        var usuario = new Usuario
        {
            NombreCompleto = dto.NombreCompleto,
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            QRCode = dto.QRCode,
            Activo = true
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return MapUsuarioDetalleBasico(usuario);
    }

    public async Task<UsuarioDetalleDto> ActualizarAsync(int id, ActualizarUsuarioDto dto)
    {
        var usuario = await _db.Usuarios.FindAsync(id)
            ?? throw new KeyNotFoundException($"Usuario con Id {id} no encontrado.");

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        if (!string.IsNullOrWhiteSpace(dto.QRCode))
        {
            usuario.QRCode = dto.QRCode;
        }

        usuario.NombreCompleto = dto.NombreCompleto;
        usuario.Email = dto.Email;

        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync();

        await _db.Entry(usuario).Collection(u => u.ProyectosAsignados).LoadAsync();
        foreach (var pur in usuario.ProyectosAsignados)
        {
            await _db.Entry(pur).Reference(p => p.Proyecto).LoadAsync();
            await _db.Entry(pur).Reference(p => p.Rol).LoadAsync();
        }

        return MapUsuarioDetalleBasico(usuario);
    }

    public async Task DesactivarAsync(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id)
            ?? throw new KeyNotFoundException($"Usuario con Id {id} no encontrado.");

        usuario.Activo = false;
        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync();
    }

    public async Task ActivarAsync(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id)
            ?? throw new KeyNotFoundException($"Usuario con Id {id} no encontrado.");
        usuario.Activo = true;
        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync();
    }

    public async Task AsignarProyectoRolAsync(int usuarioId, AsignarProyectoRolDto dto)
    {
        var usuarioExiste = await _db.Usuarios.AnyAsync(u => u.Id == usuarioId);
        if (!usuarioExiste)
            throw new KeyNotFoundException("Usuario no encontrado.");

        var proyectoExiste = await _db.Proyectos.AnyAsync(p => p.Id == dto.ProyectoId);
        if (!proyectoExiste)
            throw new KeyNotFoundException("Proyecto no encontrado.");

        var rolExiste = await _db.Roles.AnyAsync(r => r.Id == dto.RolId && r.ProyectoId == dto.ProyectoId);
        if (!rolExiste)
            throw new InvalidOperationException("El rol no pertenece al proyecto especificado.");

        var asignacion = await _db.ProyectoUsuarioRoles
            .FirstOrDefaultAsync(pur =>
                pur.UsuarioId == usuarioId &&
                pur.ProyectoId == dto.ProyectoId);

        if (asignacion != null)
        {
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
        var tieneAcceso = await _db.ProyectoUsuarioRoles
            .AnyAsync(pur => pur.UsuarioId == usuarioId
                          && pur.ProyectoId == proyectoId
                          && pur.Activo);
        if (!tieneAcceso)
            throw new InvalidOperationException("El usuario no tiene acceso activo a este proyecto.");

        var vistaIdsDistintos = vistaIds.Distinct().ToList();
        var vistasValidas = await _db.Vistas
            .Where(v => v.ProyectoId == proyectoId && vistaIdsDistintos.Contains(v.Id))
            .Select(v => v.Id)
            .ToListAsync();

        if (vistasValidas.Count != vistaIdsDistintos.Count)
            throw new InvalidOperationException("Algunas vistas no pertenecen al proyecto.");

        var accesosActuales = await _db.UsuarioVistasAcceso
            .Where(uva => uva.UsuarioId == usuarioId
                       && uva.ProyectoId == proyectoId)
            .ToListAsync();

        _db.UsuarioVistasAcceso.RemoveRange(accesosActuales);

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

    public async Task ResetearIntentosAsync(int usuarioId)
    {
        var usuario = await _db.Usuarios.FindAsync(usuarioId)
            ?? throw new KeyNotFoundException($"Usuario con Id {usuarioId} no encontrado.");

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync();
    }

    // Mapeo básico (sin datos de auditoría)
    private UsuarioDetalleDto MapUsuarioDetalleBasico(Usuario usuario)
    {
        return new UsuarioDetalleDto(
            usuario.Id,
            usuario.NombreCompleto,
            usuario.Username,
            usuario.Email,
            usuario.QRCode,
            usuario.Activo,
            usuario.UltimoAcceso ?? DateTime.MinValue,
            usuario.ProyectosAsignados?.Select(pur => new ProyectoAsignadoDto(
                pur.ProyectoId,
                pur.Proyecto?.Codigo ?? "",
                pur.Proyecto?.Nombre ?? "",
                pur.Rol?.Nombre ?? "",
                pur.Rol?.Nivel ?? 0,
                pur.Activo
            )) ?? Enumerable.Empty<ProyectoAsignadoDto>(),
            usuario.IntentosFallidos,
            usuario.BloqueadoHasta,
            null,  // CreadoPor no disponible en contexto básico
            usuario.FechaCreacion,
            null,  // ModificadoPor
            usuario.FechaModificacion
        );
    }
}