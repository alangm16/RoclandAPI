
// ITokenService.cs 
namespace RCD.SuperAdmin.Application.Interfaces;

public interface ITokenService
{
    string GenerarAccessToken(int usuarioId, string username, IEnumerable<string> roles);
    string GenerarRefreshToken();
    DateTime ObtenerExpiracionAccessToken();
}
