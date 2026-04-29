
namespace RCD.Mob.GuardiaRelevo.Domain.Entities
{
    public class ChecklistRespuesta
    {
        public int Id { get; set; }
        public int RondinId { get; set; }
        public int PuntoId { get; set; }
        public bool Respuesta { get; set; }
        public string? Comentario { get; set; }
        public DateTime FechaRespuesta { get; set; }

        public Rondin? Rondin { get; set; }
        public ChecklistPunto? Punto { get; set; }
    }
}
