using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Seguridad;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface ISesionService
{
    Task<PagedResult<SesionActivaDto>> ObtenerSesionesAsync(FiltroSesionesDto filtro);
    Task RevocarSesionAsync(int refreshTokenId);
}