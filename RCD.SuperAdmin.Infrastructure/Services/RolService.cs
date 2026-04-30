// RCD.SuperAdmin.Infrastructure/Services/RolService.cs
using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.Roles;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class RolService(SuperAdminDbContext db) : IRolService
{
    public async Task<IEnumerable<RolDto>> ObtenerTodosAsync()
    {
        return await db.Roles
            .Where(r => r.Activo)
            .OrderBy(r => r.Nombre)
            .Select(r => new RolDto(r.Id, r.Nombre, r.Activo))
            .ToListAsync();
    }

    public async Task<RolDto> CrearAsync(CrearRolRequest request)
    {
        var rol = new Rol
        {
            Nombre = request.Nombre
        };

        db.Roles.Add(rol);
        await db.SaveChangesAsync();

        return new RolDto(rol.Id, rol.Nombre, rol.Activo);
    }
}