using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Dispositivos;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface ITokenDispositivoService
{
    Task RegistrarAsync(RegistrarDispositivoRequest request, string userAgent);
    Task<PagedResult<DispositivoDto>> ObtenerPorUsuarioAsync(int usuarioId, int pagina = 1, int tamanoPagina = 10);
    Task RevocarDispositivoAsync(int dispositivoId);
}