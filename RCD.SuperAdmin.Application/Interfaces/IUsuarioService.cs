using RCD.SuperAdmin.Application.DTOs.Usuarios;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioDto>> ObtenerTodosAsync();
    Task<UsuarioDto?> ObtenerPorIdAsync(int id);
    Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request);
    Task ActualizarAsync(int id, ActualizarUsuarioRequest request);
    Task DesactivarAsync(int id);
}