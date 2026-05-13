using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RCD.SuperAdmin.Application.DTOs.Auth;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.Shared.Kernel.Settings;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class JwtService(IOptions<JwtSettings> options) : IJwtService
{
    private readonly JwtSettings _cfg = options.Value;

    public string GenerarTokenDirecto(TokenDirectoClaimsDto claims)
    {
        var claimsList = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,    claims.UsuarioId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, claims.Username),
            new("esMaestro",      "false"),
            new("proyectoId",     claims.ProyectoId.ToString()),
            new("codigoProyecto", claims.CodigoProyecto),
            new("rolId",          claims.RolId.ToString()),
            new("nombreRol",      claims.NombreRol),
            new(ClaimTypes.Role,  claims.NombreRol),
            new("nivelRol",       claims.NivelRol.ToString()),
            new("plataforma",     claims.Plataforma),
        };

        return GenerarToken(claimsList, _cfg.ExpirationMinutes);
    }

    public string GenerarTokenMaestro(TokenMaestroClaimsDto claims)
    {
        var claimsList = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, claims.UsuarioId.ToString()),
        new(JwtRegisteredClaimNames.UniqueName, claims.Username),
        new("esMaestro", "true"),
        new(ClaimTypes.Role, claims.Rol),          // ← rol del proyecto super-admin
        new("nivel", claims.Nivel.ToString()),     // ← nivel del rol
        new("plataforma", claims.Plataforma),
    };

        return GenerarToken(claimsList, _cfg.MaestroExpirationMinutes);
    }

    public string GenerarRefreshToken()
    {
        // GUID aleatorio + hash SHA-256 → opaco, no reversible
        var random = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(random);
    }

    public TokenClaimsDto ValidarToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg.SecretKey));
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _cfg.Issuer,
            ValidateAudience = true,
            ValidAudience = _cfg.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = handler.ValidateToken(token, parameters, out _);

        return new TokenClaimsDto(
            UsuarioId: int.Parse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!),
            Username: principal.FindFirstValue(JwtRegisteredClaimNames.UniqueName)!,
            EsMaestro: principal.FindFirstValue("esMaestro") == "true",
            ProyectoId: TryParseInt(principal.FindFirstValue("proyectoId")),
            CodigoProyecto: principal.FindFirstValue("codigoProyecto"),
            NombreRol: principal.FindFirstValue(ClaimTypes.Role),           // ← rol general
            NivelRol: TryParseInt(principal.FindFirstValue("nivel")),       // ← nivel general
            Plataforma: principal.FindFirstValue("plataforma") ?? "Web"
        );
    }

    private string GenerarToken(IEnumerable<Claim> claims, int expirationMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _cfg.Issuer,
            audience: _cfg.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static int? TryParseInt(string? value) =>
        int.TryParse(value, out var result) ? result : null;
}