namespace RCD.SuperAdmin.Domain.Entities;

public class Alerta
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public bool Resuelta { get; set; }
    public string? AccionUrl { get; set; }
}
