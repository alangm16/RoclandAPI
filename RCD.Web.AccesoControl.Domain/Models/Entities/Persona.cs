using RCD.Web.AccesoControl.Domain.Models.Entities.Base;
using RCD.Web.AccesoControl.Domain.Models.Entities;

namespace RCD.Web.AccesoControl.Domain.Models.Entities;
public class Persona : AuditableEntity
{
    public string Nombre { get; set; } = null!;
    public int TipoIdentificacionId { get; set; }
    public string NumeroIdentificacion { get; set; } = null!;
    public string? Empresa { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? FechaUltimaVisita { get; set; }
    public int TotalVisitas { get; set; } = 0;
    public TipoIdentificacion TipoIdentificacion { get; set; } = null!;
    public ICollection<RegistroVisitante> RegistrosVisitantes { get; set; } = [];
    public ICollection<RegistroProveedor> RegistrosProveedores { get; set; } = [];
    public ICollection<SolicitudPendiente> Solicitudes { get; set; } = [];
}