
using System.Numerics;

namespace RCD.SuperAdmin.Domain.Entities
{
    public class Vista
    {
        public int Id { get; set; }
        public int ProyectoId { get; set; }
        public Proyecto Proyecto { get; set; } = null!;
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Icono { get; set; }
        public bool Activo { get; set; } = true;
        public int Orden { get; set; }

        public ICollection<PermisoUsuario> Permisos { get; set; } = [];
    }
}
