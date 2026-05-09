using RCD.AccesoControlWeb.Application.DTOs;

public interface IAuthService
{
    Task<PerfilContextoDto?> ObtenerPerfilContextoAsync(int superAdminUsuarioId);
}