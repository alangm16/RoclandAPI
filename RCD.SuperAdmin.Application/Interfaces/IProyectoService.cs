using RCD.SuperAdmin.Application.DTOs;
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
    Task DesactivarRolAsync(int proyectoId, int rolId);
    Task ActivarRolAsync(int proyectoId, int rolId);
    Task<IEnumerable<VistaDto>> ObtenerVistasAsync(int proyectoId);
    Task<VistaDto> CrearVistaAsync(int proyectoId, CrearVistaDto dto);
    Task DesactivarVistaAsync(int proyectoId, int vistaId);
    Task ActivarVistaAsync(int proyectoId, int vistaId);
    Task<ProyectoDetalleDto?> ObtenerPorCodigoAsync(string codigo);
    Task<RolDto> ActualizarRolAsync(int proyectoId, int rolId, ActualizarRolDto dto);
    Task<VistaDto> ActualizarVistaAsync(int proyectoId, int vistaId, ActualizarVistaDto dto);
    Task<PagedResult<UsuarioProyectoDto>> ObtenerUsuariosPorProyectoAsync(int proyectoId, int pagina, int tamanoPagina);
    Task ActivarAsync(int id);
    Task ReordenarAsync(IEnumerable<ProyectoOrdenDto> items);
}