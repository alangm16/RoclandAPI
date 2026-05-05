namespace RCD.Web.AccesoControl.Application.DTOs
{
    public class PerfilDto
    {
        public int Id { get; set; }
        public int SuperAdminUsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? NumeroEmpleado { get; set; }
        public string TipoPerfil { get; set; } = string.Empty; // 'Guardia', 'Administrador', etc.
        public string? Turno { get; set; }
        public bool Activo { get; set; }
    }
}