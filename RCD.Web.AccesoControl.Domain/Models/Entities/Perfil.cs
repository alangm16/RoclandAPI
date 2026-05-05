namespace RCD.Web.AccesoControl.Domain.Models.Entities
{
    public class Perfil
    {
        public int Id { get; set; }
        public int SuperAdminUsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? NumeroEmpleado { get; set; }
        public string TipoPerfil { get; set; } = string.Empty;
        public string? Turno { get; set; }
        public string? DeviceToken { get; set; }
        public string? FcmToken { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        // Propiedades de navegación
        public ICollection<RegistroVisitante> RegistrosVisitantesEntrada { get; set; } = new List<RegistroVisitante>();
        public ICollection<RegistroVisitante> RegistrosVisitantesSalida { get; set; } = new List<RegistroVisitante>();
        public ICollection<RegistroProveedor> RegistrosProveedoresEntrada { get; set; } = new List<RegistroProveedor>();
        public ICollection<RegistroProveedor> RegistrosProveedoresSalida { get; set; } = new List<RegistroProveedor>();
    }
}