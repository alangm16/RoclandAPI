using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.RolesSA;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class RolSAService : IRolSAService
{
    private readonly SuperAdminDbContext _db;

    public RolSAService(SuperAdminDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<RolSADto>> ObtenerTodosAsync()
    {
        return await _db.RolesSA
            .OrderBy(r => r.Nivel)
            .ThenBy(r => r.Nombre)
            .Select(r => new RolSADto(
                r.Id,
                r.Nombre,
                r.Nivel,
                r.Descripcion,
                r.Activo
            ))
            .ToListAsync();
    }

    public async Task<RolSADto> ObtenerPorIdAsync(int id)
    {
        var rol = await _db.RolesSA.FindAsync(id)
            ?? throw new KeyNotFoundException($"Rol SA con Id {id} no encontrado.");

        return new RolSADto(rol.Id, rol.Nombre, rol.Nivel, rol.Descripcion, rol.Activo);
    }

    public async Task<RolSADto> CrearAsync(CrearRolSADto dto)
    {
        if (await _db.RolesSA.AnyAsync(r => r.Nombre == dto.Nombre))
            throw new InvalidOperationException($"Ya existe un Rol SA con el nombre '{dto.Nombre}'.");

        var rol = new RolSA
        {
            Nombre = dto.Nombre,
            Nivel = dto.Nivel,
            Descripcion = dto.Descripcion,
            Activo = true
        };

        _db.RolesSA.Add(rol);
        await _db.SaveChangesAsync();

        return new RolSADto(rol.Id, rol.Nombre, rol.Nivel, rol.Descripcion, rol.Activo);
    }

    public async Task<RolSADto> ActualizarAsync(int id, ActualizarRolSADto dto)
    {
        var rol = await _db.RolesSA.FindAsync(id)
            ?? throw new KeyNotFoundException($"Rol SA con Id {id} no encontrado.");

        if (dto.Nombre is not null && dto.Nombre != rol.Nombre)
        {
            if (await _db.RolesSA.AnyAsync(r => r.Nombre == dto.Nombre && r.Id != id))
                throw new InvalidOperationException($"Ya existe un Rol SA con el nombre '{dto.Nombre}'.");
            rol.Nombre = dto.Nombre;
        }

        if (dto.Nivel.HasValue)
            rol.Nivel = dto.Nivel.Value;

        if (dto.Descripcion is not null)
            rol.Descripcion = dto.Descripcion;

        if (dto.Activo.HasValue)
            rol.Activo = dto.Activo.Value;

        _db.RolesSA.Update(rol);
        await _db.SaveChangesAsync();

        return new RolSADto(rol.Id, rol.Nombre, rol.Nivel, rol.Descripcion, rol.Activo);
    }

    public async Task DesactivarAsync(int id)
    {
        var rol = await _db.RolesSA.FindAsync(id)
            ?? throw new KeyNotFoundException($"Rol SA con Id {id} no encontrado.");

        rol.Activo = false;
        _db.RolesSA.Update(rol);
        await _db.SaveChangesAsync();
    }
}