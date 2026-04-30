
namespace RCD.SuperAdmin.Domain.Entities
{
    public class Proyecto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Plataforma { get; set; } = string.Empty;
        public string? IconoCss { get; set; }
        public string? UrlBase { get; set; }
        public bool Activo { get; set; } = true;
        public int Orden { get; set; }

        public ICollection<Vista> Vistas { get; set; } = [];
        public ICollection<PermisoRol> PermisosRol { get; set; } = [];
        public ICollection<PermisoUsuario> PermisosUsuario { get; set; } = [];
    }
}
