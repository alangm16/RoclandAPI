
namespace RCD.Shared.Infrastructure.Security
{
    public interface IJwtTokenService
    {
        string GenerarToken(int usuarioId, string username, IEnumerable<string> roles);
    }
}
