using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Domain.Base;

namespace RCD.SuperAdmin.Domain.Entities;

public class Usuario : AuditableEntity
{
    public string NombreCompleto { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = null!;
    public string? QRCode { get; set; }
    public int? RolSAId { get; set; }
    public int IntentosFallidos { get; set; } = 0;
    public DateTime? BloqueadoHasta { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public RolSA? RolSA { get; set; }

    public ICollection<ProyectoUsuarioRol> ProyectosAsignados { get; set; } = [];
    public ICollection<UsuarioVistaAcceso> VistasAcceso { get; set; } = [];
    public ICollection<TokenDispositivo> TokensDispositivo { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<LogAcceso> LogsAcceso { get; set; } = [];
}