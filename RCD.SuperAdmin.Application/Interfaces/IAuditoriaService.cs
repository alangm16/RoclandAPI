using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Auditoria;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IAuditoriaService
{
    Task<PagedResult<AuditoriaDto>> ObtenerRegistrosAsync(FiltroAuditoriaDto filtro);
}