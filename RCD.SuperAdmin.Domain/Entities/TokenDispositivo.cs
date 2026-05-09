namespace RCD.SuperAdmin.Domain.Entities;

public class TokenDispositivo
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int ProyectoId { get; set; }
    public string Plataforma { get; set; } = null!;
    public string? DeviceToken { get; set; }
    public string? FcmToken { get; set; }
    public string? DispositivoInfo { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;
    public Usuario Usuario { get; set; } = null!;
    public Proyecto Proyecto { get; set; } = null!;
}