using RCD.SuperAdmin.Application.DTOs.Auth;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IJwtService
{
    string GenerarTokenDirecto(TokenDirectoClaimsDto claims);
    string GenerarTokenMaestro(TokenMaestroClaimsDto claims);
    TokenClaimsDto ValidarToken(string token);
    string GenerarRefreshToken();
}