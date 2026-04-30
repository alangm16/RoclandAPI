using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Proyectos;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IProyectoService
{
    Task<IEnumerable<ProyectoDetalleDto>> ObtenerTodosAsync();
    Task<ProyectoDetalleDto> CrearProyectoAsync(CrearProyectoRequest request);
    Task<ProyectoDetalleDto> CrearVistaAsync(int proyectoId, CrearVistaRequest request);
}
