using RCD.SuperAdmin.Domain.Base;

namespace RCD.SuperAdmin.Domain.Entities;
public class UsuarioVistaAcceso : AuditableEntity
{
    public int UsuarioId { get; set; }
    public int ProyectoId { get; set; }
    public int VistaId { get; set; }
    public bool TieneAcceso { get; set; } = true;
    public Usuario Usuario { get; set; } = null!;
    public Proyecto Proyecto { get; set; } = null!;
    public Vista Vista { get; set; } = null!;
    public Usuario? CreadoPorUsuario { get; set; }
    public Usuario? ModificadoPorUsuario { get; set; }
}