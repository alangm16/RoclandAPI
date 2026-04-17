using RCD.Web.AccesoControl.Application.DTOs;

namespace RCD.Web.AccesoControl.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginGuardiaAsync(LoginRequest request);
    Task<LoginResponse?> LoginAdminAsync(LoginRequest request);
}