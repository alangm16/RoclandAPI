namespace RCD.SuperAdmin.Domain.Base;

public abstract class AuditableEntity
{
    public int Id { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public int? CreadoPor { get; set; }
    public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;
    public int? ModificadoPor { get; set; }
}