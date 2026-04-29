
namespace RCD.Mob.GuardiaRelevo.Domain.Entities
{
    public class ChecklistPunto
    {
        public int Id { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; } = true;
    }
}
