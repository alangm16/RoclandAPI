namespace RCD.SuperAdmin.Domain.Entities
{
    public class PermisoUsuario
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int ProyectoId { get; set; }
        public Proyecto Proyecto { get; set; } = null!;

        public int? VistaId { get; set; }
        public Vista? Vista { get; set; }
    }
}
