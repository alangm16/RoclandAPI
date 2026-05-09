namespace RCD.Web.AccesoControl.Domain.Models.Entities;
public class SolicitudPendiente
{
    public int Id { get; set; }
    public string TipoRegistro { get; set; } = null!;
    public int RegistroId { get; set; }
    public int PersonaId { get; set; }
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "Pendiente";
    public int? PerfilId { get; set; }
    public Persona Persona { get; set; } = null!;
    public Perfil? PerfilResolutor { get; set; }
}