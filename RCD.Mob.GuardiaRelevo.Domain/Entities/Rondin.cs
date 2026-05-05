

namespace RCD.Mob.GuardiaRelevo.Domain.Entities;

public class Rondin
{
    public int Id { get; set; }

    // Aquí usamos el nombre exacto de la columna en tu BD
    public int RelevodId { get; set; }

    public string TipoRondin { get; set; } = null!; // 'Entrega' o 'Verificacion'
    public int GuardiaId { get; set; }
    public string Estado { get; set; } = "EnCurso";
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Observaciones { get; set; }

    // Propiedades de navegación
    public Relevo Relevo { get; set; } = null!;
    public ICollection<ChecklistRespuesta> Respuestas { get; set; } = new List<ChecklistRespuesta>();
}