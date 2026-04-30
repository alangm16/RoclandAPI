
namespace RCD.SuperAdmin.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public ICollection<UsuarioRol> Roles { get; set; } = [];
        public ICollection<PermisoUsuario> Permisos { get; set; } = [];
    }
}
