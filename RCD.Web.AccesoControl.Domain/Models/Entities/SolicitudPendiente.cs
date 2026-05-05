namespace RCD.Web.AccesoControl.Domain.Models.Entities
{
    public class SolicitudPendiente
    {
        public int Id { get; set; }
        public string TipoRegistro { get; set; } = string.Empty; // "Visitante" | "Proveedor"
        public int RegistroId { get; set; }
        public int PersonaId { get; set; }
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "Pendiente";

        // FK actualizada a Perfil
        public int? PerfilId { get; set; }

        // Propiedades de navegación
        public Persona Persona { get; set; } = null!;
        public Perfil? Perfil { get; set; }
    }
}