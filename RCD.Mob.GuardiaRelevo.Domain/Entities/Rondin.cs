

namespace RCD.Mob.GuardiaRelevo.Domain.Entities
{
    public class Rondin
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public string Turno { get; set; } = string.Empty;
        public TimeOnly HoraInicio { get; set; }
        public int GuardiaSalienteId { get; set; }
        public int GuardiaEntranteId { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string? NotasFinales { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Navegacion
        public Usuario? GuardiaSaliente { get; set; }
        public Usuario? GuardiaEntrante { get; set; }
        public ICollection<RondinEvento> Eventos { get; set; } = new List<RondinEvento>();
        public ICollection<ChecklistRespuesta> Respuestas { get; set; } = new List<ChecklistRespuesta>();
    }
}
