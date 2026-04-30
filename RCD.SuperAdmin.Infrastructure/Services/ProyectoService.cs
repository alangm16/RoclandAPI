// ProyectoService.cs
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Infrastructure.Data;
using RCD.SuperAdmin.Application.DTOs.Proyectos;
using Microsoft.EntityFrameworkCore;

public class ProyectoService(SuperAdminDbContext db) : IProyectoService
{
    public async Task<IEnumerable<ProyectoDetalleDto>> ObtenerTodosAsync()
    {
        return await db.Proyectos
            .Where(p => p.Activo)
            .Include(p => p.Vistas.Where(v => v.Activo))
            .OrderBy(p => p.Orden)
            .Select(p => new ProyectoDetalleDto(
                p.Id,
                p.Codigo,
                p.Nombre,
                p.Plataforma,
                p.UrlBase,
                p.IconoCss,
                p.Orden,
                p.Activo,
                p.Vistas.Select(v => new VistaDetalleDto(v.Id, v.Codigo, v.Nombre, v.Icono, v.Orden))))
            .ToListAsync();
    }

    public async Task<ProyectoDetalleDto> CrearProyectoAsync(CrearProyectoRequest request)
    {
        var proyecto = new Proyecto
        {
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            Plataforma = request.Plataforma,
            UrlBase = request.UrlBase,
            IconoCss = request.IconoCss,
            Orden = request.Orden
        };
        db.Proyectos.Add(proyecto);
        await db.SaveChangesAsync();
        return (await ObtenerTodosAsync()).First(p => p.Id == proyecto.Id);
    }

    public async Task<ProyectoDetalleDto> CrearVistaAsync(int proyectoId, CrearVistaRequest request)
    {
        db.Vistas.Add(new Vista
        {
            ProyectoId = proyectoId,
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            Icono = request.Icono,
            Orden = request.Orden
        });
        await db.SaveChangesAsync();
        return (await ObtenerTodosAsync()).First(p => p.Id == proyectoId);
    }
}