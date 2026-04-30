
namespace RCD.SuperAdmin.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public DateTime FechaExpiracion { get; set; }
        public bool Revocado { get; set; } = false;
        public string? IpCreacion { get; set; }
        public string? DispositivoInfo { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
