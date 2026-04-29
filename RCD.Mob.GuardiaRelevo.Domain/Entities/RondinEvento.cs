
namespace RCD.Mob.GuardiaRelevo.Domain.Entities
{
    public class RondinEvento
    {
        public int Id { get; set; }
        public int RondinId { get; set; }
        public int UsuarioId { get; set; }
        public string TipoEvento { get; set; } = string.Empty;
        public string TipoGuardia { get; set; } = string.Empty;
        public string? FirmaBase64 { get; set; }
        public bool Exitoso { get; set; } = true;
        public DateTime FechaEvento { get; set; } 
        
        public Rondin? Rondin { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
