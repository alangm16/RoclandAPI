namespace RCD.Mob.GuardiaRelevo.Domain.Entities;

public class Usuario
{
    // Esta es tu nueva llave primaria, que es exactamente el ID de SuperAdmin
    public int SuperAdminUsuarioId { get; set; }

    public string NumeroEmpleado { get; set; } = null!;

    public string RolLocal { get; set; } = null!; // "Supervisor" o "Guardia"

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    // (Opcional) Propiedades de navegación si EF Core las necesita para los Rondines e Incidencias
    // public ICollection<Rondin> Rondines { get; set; } = new List<Rondin>();
}
