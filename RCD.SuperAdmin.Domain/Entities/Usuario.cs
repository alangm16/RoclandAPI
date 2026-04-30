
namespace RCD.SuperAdmin.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? QRCode { get; set; }
        public string? DeviceToken { get; set; }
        public string? FcmToken { get; set; }
        public int IntentosFallidos { get; set; } = 0;
        public DateTime? BloqueadoHasta { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public ICollection<UsuarioRol> Roles { get; set; } = [];
        public ICollection<PermisoUsuario> Permisos { get; set; } = [];
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
        public ICollection<LogAcceso> Logs { get; set; } = [];
    }
}
