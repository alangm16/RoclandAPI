
namespace RCD.Mob.GuardiaRelevo.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Usuario_ { get; set; } = string.Empty;
        public string NumeroEmpleado { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? DeviceToken { get; set; }
        public string? FcmToken { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } 
    }
}
