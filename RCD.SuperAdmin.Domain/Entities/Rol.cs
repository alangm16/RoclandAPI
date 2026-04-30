
namespace RCD.SuperAdmin.Domain.Entities
{
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        public ICollection<UsuarioRol> Usuarios { get; set; } = [];
    }
}
