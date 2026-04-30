using RCD.SuperAdmin.Application.DTOs.Roles;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IRolService
{
    Task<IEnumerable<RolDto>> ObtenerTodosAsync();
    Task<RolDto> CrearAsync(CrearRolRequest request);
}