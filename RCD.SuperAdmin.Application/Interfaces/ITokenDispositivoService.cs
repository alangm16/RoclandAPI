using RCD.SuperAdmin.Application.DTOs.Dispositivos;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface ITokenDispositivoService
{
    Task RegistrarAsync(RegistrarDispositivoRequest request);
}