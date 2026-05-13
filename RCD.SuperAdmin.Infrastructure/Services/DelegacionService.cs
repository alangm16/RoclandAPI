using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Delegaciones;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class DelegacionService : IDelegacionService
{
    private readonly SuperAdminDbContext _db;

    public DelegacionService(SuperAdminDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<DelegacionDto>> ObtenerDelegacionesAsync(FiltroDelegacionesDto filtro)
    {
        var query = _db.ProyectoUsuarioRoles
            .Include(pur => pur.Usuario)
            .Include(pur => pur.Proyecto)
            .Include(pur => pur.Rol)
            .Include(pur => pur.CreadoPorUsuario)
            .Where(pur => pur.Activo && pur.CreadoPor != null)
            .AsQueryable();

        if (filtro.ProyectoId.HasValue)
            query = query.Where(pur => pur.ProyectoId == filtro.ProyectoId.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(pur => pur.FechaCreacion)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(pur => new DelegacionDto(
                pur.CreadoPorUsuario != null ? pur.CreadoPorUsuario.Username : "Sistema",
                pur.Usuario.Username,
                pur.Proyecto.Codigo,
                pur.Proyecto.Nombre,
                pur.Rol.Nombre,
                pur.FechaCreacion
            ))
            .ToListAsync();

        return new PagedResult<DelegacionDto>(
            items,
            total,
            filtro.Pagina,
            filtro.TamanoPagina
        );
    }
}