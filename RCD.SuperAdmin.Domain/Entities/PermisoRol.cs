
namespace RCD.SuperAdmin.Domain.Entities
{
    public class PermisoRol
    {
        public int Id { get; set; }
        public int RolId { get; set; }
        public Rol Rol { get; set; } = null!;
        public int ProyectoId { get; set; }
        public Proyecto Proyecto { get; set; } = null!;
        public int? VistaId { get; set; }
        public Vista? Vista { get; set; }

        // CRUD granular
        public bool PuedeLeer { get; set; } = true;
        public bool PuedeCrear { get; set; } = false;
        public bool PuedeEditar { get; set; } = false;
        public bool PuedeBorrar { get; set; } = false;
    }
}
