using RCD.Web.AccesoControl.Domain.Models.Entities.Base;
using RCD.Web.AccesoControl.Domain.Models.Entities;

namespace RCD.Web.AccesoControl.Domain.Models.Entities;

public class Area : AuditableEntity
{
    public string Nombre { get; set; } = null!;
    public ICollection<RegistroVisitante> RegistrosVisitantes { get; set; } = [];
}
