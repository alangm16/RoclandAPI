using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Services;

public class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, DateTime Expiracion) Generar(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddHours(
                          double.Parse(_config["Jwt:ExpirationHours"] ?? "12"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,  usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, usuario.NombreCompleto),
            new Claim("usuario",                    usuario.Usuario_),
            new Claim(ClaimTypes.Role,              usuario.Rol),
            new Claim("qr",                         usuario.QRCode),
            new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}