using RCD.SuperAdmin.Application.DTOs.Proyectos;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IProyectoService
{
    Task<IEnumerable<ProyectoListDto>> ObtenerTodosAsync();
    Task<ProyectoDetalleDto?> ObtenerPorIdAsync(int id);
    Task<ProyectoDetalleDto> CrearAsync(CrearProyectoDto dto);
    Task<ProyectoDetalleDto> ActualizarAsync(int id, ActualizarProyectoDto dto);
    Task DesactivarAsync(int id);
    Task<IEnumerable<RolDto>> ObtenerRolesAsync(int proyectoId);
    Task<RolDto> CrearRolAsync(int proyectoId, CrearRolDto dto);
    Task EliminarRolAsync(int proyectoId, int rolId);
    Task<IEnumerable<VistaDto>> ObtenerVistasAsync(int proyectoId);
    Task<VistaDto> CrearVistaAsync(int proyectoId, CrearVistaDto dto);
    Task EliminarVistaAsync(int proyectoId, int vistaId);
}