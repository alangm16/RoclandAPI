namespace RCD.Web.AccesoControl.Domain.Models.Entities.Base;

public abstract class AuditableEntity
{
    public int Id { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public int? CreadoPor { get; set; }  // SuperAdminUsuarioId
    public DateTime? FechaModificacion { get; set; }
    public int? ModificadoPor { get; set; }  // SuperAdminUsuarioId
}