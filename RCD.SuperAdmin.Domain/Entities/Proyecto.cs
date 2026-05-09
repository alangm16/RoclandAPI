using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Domain.Base;

namespace RCD.SuperAdmin.Domain.Entities;

public class Proyecto : AuditableEntity
{
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Plataforma { get; set; } = null!;
    public string? IconoCss { get; set; } = "bi-box";
    public string? UrlBase { get; set; }
    public string? Version { get; set; } = "1.0.0";
    public string Estado { get; set; } = "Produccion";
    public string? Descripcion { get; set; }
    public int Orden { get; set; } = 0;
    public ICollection<Rol> Roles { get; set; } = [];
    public ICollection<Vista> Vistas { get; set; } = [];
    public ICollection<ProyectoUsuarioRol> UsuariosAsignados { get; set; } = [];
    public ICollection<TokenDispositivo> TokensDispositivo { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<LogAcceso> LogsAcceso { get; set; } = [];
    public ICollection<Alerta> Alertas { get; set; } = [];
}