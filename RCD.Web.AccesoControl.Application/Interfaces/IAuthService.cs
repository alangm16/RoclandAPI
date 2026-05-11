using RCD.Web.AccesoControl.Application.DTOs;

public interface IAuthService
{
    Task<PerfilContextoDto?> ObtenerPerfilContextoAsync(int superAdminUsuarioId);
}