using RCD.Web.AccesoControl.Domain.Models.Entities.Base;
using RCD.Web.AccesoControl.Domain.Models.Entities;

namespace RCD.Web.AccesoControl.Domain.Models.Entities;

public class Gafete : AuditableEntity
{
    public string Codigo { get; set; } = null!;
    public string Estado { get; set; } = "Libre";
    public ICollection<RegistroVisitante> RegistrosVisitantes { get; set; } = [];
    public ICollection<RegistroProveedor> RegistrosProveedores { get; set; } = [];
}