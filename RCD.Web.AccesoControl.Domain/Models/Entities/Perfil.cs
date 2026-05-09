using RCD.Web.AccesoControl.Domain.Models.Entities.Base;
using RCD.Web.AccesoControl.Domain.Models.Entities;

namespace RCD.Web.AccesoControl.Domain.Models.Entities;

public class Perfil : AuditableEntity
{
    public int SuperAdminUsuarioId { get; set; }
    public string NombreCompleto { get; set; } = null!;
    public string? NumeroEmpleado { get; set; }
    public string? Turno { get; set; }
    public ICollection<RegistroVisitante> EntradasVisitantes { get; set; } = [];
    public ICollection<RegistroVisitante> SalidasVisitantes { get; set; } = [];
    public ICollection<RegistroProveedor> EntradasProveedores { get; set; } = [];
    public ICollection<RegistroProveedor> SalidasProveedores { get; set; } = [];
    public ICollection<SolicitudPendiente> SolicitudesResueltas { get; set; } = [];
}