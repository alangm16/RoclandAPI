using RCD.Mob.GuardiaRelevo.Application.DTOs;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using RCD.Mob.GuardiaRelevo.Domain.Interfaces;
using RCD.Shared.Infrastructure.Security;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Services;

public class AuthService/* :*/ /*IAuthService*/
{
    private readonly IUsuarioRepository _usuarios;
    private readonly JwtTokenService _jwt;

    public AuthService(IUsuarioRepository usuarios, JwtTokenService jwt)
    {
        _usuarios = usuarios;
        _jwt = jwt;
    }

    //public async Task<LoginResponseDto?> LoginAsync(string usuario, string password, CancellationToken ct = default)
    //{
    //    var user = await _usuarios.ObtenerPorUsuarioAsync(usuario, ct);

    //    if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
    //        return null;

    //    //var (token, expiracion) = _jwt.Generar(user);


    //    return new LoginResponseDto(
    //        user.Id,
    //        user.NombreCompleto,
    //        user.Usuario_,
    //        user.Rol,
    //        user.QRCode,
    //        token,
    //        expiracion
    //    );
    //}
}