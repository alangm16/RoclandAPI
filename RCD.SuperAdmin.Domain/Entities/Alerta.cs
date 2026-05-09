namespace RCD.SuperAdmin.Domain.Entities;
public class Alerta
{
    public int Id { get; set; }
    public int? ProyectoId { get; set; }
    public string Tipo { get; set; } = null!;
    public string Titulo { get; set; } = null!;
    public string Mensaje { get; set; } = null!;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public bool Resuelta { get; set; } = false;
    public string? AccionUrl { get; set; }
    public Proyecto? Proyecto { get; set; }
}