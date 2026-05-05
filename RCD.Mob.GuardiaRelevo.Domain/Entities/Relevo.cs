namespace RCD.Mob.GuardiaRelevo.Domain.Entities;

public class Relevo
{
    public int Id { get; set; }
    public int ConfigTurnoId { get; set; }
    public DateTime Fecha { get; set; }
    public int GuardiaSalienteId { get; set; }
    public int? GuardiaEntranteId { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente, EnCurso, Completado, Incompleto
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    // Propiedades de navegación (Entity Framework las usa para los JOINs automáticos)
    public Usuario GuardiaSaliente { get; set; } = null!;
    public Usuario? GuardiaEntrante { get; set; }

    // Un relevo tiene sus rondines asociados (Entrega y Verificación)
    public ICollection<Rondin> Rondines { get; set; } = new List<Rondin>();
}