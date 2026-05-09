namespace RCD.SuperAdmin.Domain.Entities;

public class LogAcceso
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public int? ProyectoId { get; set; }
    public string UsernameUsado { get; set; } = null!;
    public bool Exitoso { get; set; }
    public string? IpAddress { get; set; }
    public string? Plataforma { get; set; }
    public string? Detalle { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public Usuario? Usuario { get; set; }
    public Proyecto? Proyecto { get; set; }
}