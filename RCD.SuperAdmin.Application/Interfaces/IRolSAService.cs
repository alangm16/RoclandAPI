using RCD.SuperAdmin.Application.DTOs.RolesSA;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IRolSAService
{
    Task<IEnumerable<RolSADto>> ObtenerTodosAsync();
    Task<RolSADto> ObtenerPorIdAsync(int id);
    Task<RolSADto> CrearAsync(CrearRolSADto dto);
    Task<RolSADto> ActualizarAsync(int id, ActualizarRolSADto dto);
    Task DesactivarAsync(int id);
}