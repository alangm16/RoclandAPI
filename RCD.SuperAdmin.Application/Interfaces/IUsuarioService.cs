using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Usuarios;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IUsuarioService
{
    Task<PagedResult<UsuarioListDto>> ObtenerTodosAsync(bool soloPanel = false, int pagina = 1, int tamanoPagina = 20, bool? activo = null);
    Task<UsuarioDetalleDto?> ObtenerPorIdAsync(int id);
    Task<UsuarioDetalleDto> CrearAsync(CrearUsuarioDto dto);
    Task<UsuarioDetalleDto> ActualizarAsync(int id, ActualizarUsuarioDto dto);
    Task DesactivarAsync(int id);
    Task ActivarAsync(int id);
    Task AsignarProyectoRolAsync(int usuarioId, AsignarProyectoRolDto dto);
    Task RevocarProyectoAsync(int usuarioId, int proyectoId);
    Task ActualizarVistasAccesoAsync(int usuarioId, int proyectoId, IEnumerable<int> vistaIds);
    Task ResetearIntentosAsync(int usuarioId);
}