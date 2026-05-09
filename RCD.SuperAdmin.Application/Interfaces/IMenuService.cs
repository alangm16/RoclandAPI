using RCD.SuperAdmin.Application.DTOs.Menu;

namespace RCD.SuperAdmin.Application.Interfaces;

/// Menús dinámicos.
/// Devuelve las vistas visibles para un usuario dentro de un proyecto,
/// según su rol y la tabla UsuarioVistaAcceso.
public interface IMenuService
{
    Task<IEnumerable<VistaMenuDto>> ObtenerMenuAsync(int usuarioId, int proyectoId);
}