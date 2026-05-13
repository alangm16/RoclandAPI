using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Seguridad;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface ILogAccesoService
{
    Task<PagedResult<LogAccesoDto>> ObtenerLogsAsync(FiltroLogsDto filtro);
}