// UsuarioService.cs
using RCD.SuperAdmin.Application.DTOs.Usuarios;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class UsuarioService(SuperAdminDbContext db) : IUsuarioService
{
    public async Task<IEnumerable<UsuarioDto>> ObtenerTodosAsync()
    {
        return await db.Usuarios
            .Where(u => u.Activo)
            .Include(u => u.Roles).ThenInclude(ur => ur.Rol)
            .Select(u => new UsuarioDto(
                u.Id, u.NombreCompleto, u.Username, u.Email,
                u.Activo, u.UltimoAcceso,
                u.Roles.Select(ur => ur.Rol.Nombre)))
            .ToListAsync();
    }

    public async Task<UsuarioDto?> ObtenerPorIdAsync(int id)
    {
        var u = await db.Usuarios
            .Include(u => u.Roles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (u is null) return null;
        return new UsuarioDto(u.Id, u.NombreCompleto, u.Username,
            u.Email, u.Activo, u.UltimoAcceso,
            u.Roles.Select(ur => ur.Rol.Nombre));
    }

    public async Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request)
    {
        var usuario = new Usuario
        {
            NombreCompleto = request.NombreCompleto,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        foreach (var rolId in request.RolIds)
            db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.Id, RolId = rolId });
        await db.SaveChangesAsync();

        return (await ObtenerPorIdAsync(usuario.Id))!;
    }

    public async Task ActualizarAsync(int id, ActualizarUsuarioRequest request)
    {
        var usuario = await db.Usuarios
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        usuario.NombreCompleto = request.NombreCompleto;
        usuario.Email = request.Email;
        usuario.Activo = request.Activo;
        usuario.FechaModificacion = DateTime.UtcNow;

        // Sincronizar roles
        db.UsuarioRoles.RemoveRange(usuario.Roles);
        foreach (var rolId in request.RolIds)
            db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = id, RolId = rolId });

        await db.SaveChangesAsync();
    }

    public async Task DesactivarAsync(int id)
    {
        var u = await db.Usuarios.FindAsync(id)
                ?? throw new KeyNotFoundException("Usuario no encontrado");
        u.Activo = false;
        u.FechaModificacion = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}