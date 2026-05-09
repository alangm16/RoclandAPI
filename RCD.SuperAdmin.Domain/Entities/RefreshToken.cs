namespace RCD.SuperAdmin.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int? ProyectoId { get; set; }
    public string Plataforma { get; set; } = "Web";
    public string Token { get; set; } = null!;
    public DateTime FechaExpiracion { get; set; }
    public bool Revocado { get; set; } = false;
    public string? IpCreacion { get; set; }
    public string? DispositivoInfo { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public Usuario Usuario { get; set; } = null!;
    public Proyecto? Proyecto { get; set; }
}