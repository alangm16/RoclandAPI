using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Alertas;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IAlertaService
{
    Task<PagedResult<AlertaDto>> ObtenerAlertasAsync(FiltroAlertasDto filtro);
    Task MarcarResueltaAsync(int alertaId);
}