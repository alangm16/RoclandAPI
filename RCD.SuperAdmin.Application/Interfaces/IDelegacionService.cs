using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Delegaciones;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IDelegacionService
{
    Task<PagedResult<DelegacionDto>> ObtenerDelegacionesAsync(FiltroDelegacionesDto filtro);
}