using RCD.SuperAdmin.Application.DTOs.Usuarios;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioListDto>> ObtenerTodosAsync();
    Task<UsuarioDetalleDto?> ObtenerPorIdAsync(int id);
    Task<UsuarioDetalleDto> CrearAsync(CrearUsuarioDto dto);
    Task<UsuarioDetalleDto> ActualizarAsync(int id, ActualizarUsuarioDto dto);
    Task DesactivarAsync(int id);
    Task AsignarProyectoRolAsync(int usuarioId, AsignarProyectoRolDto dto);
    Task RevocarProyectoAsync(int usuarioId, int proyectoId);
    Task ActualizarVistasAccesoAsync(int usuarioId, int proyectoId, IEnumerable<int> vistaIds);
}