using RCD.SuperAdmin.Domain.Base;

namespace RCD.SuperAdmin.Domain.Entities;

public class ProyectoUsuarioRol : AuditableEntity
{
    public int ProyectoId { get; set; }
    public int UsuarioId { get; set; }
    public int RolId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
    public Usuario? CreadoPorUsuario { get; set; }
    public Usuario? ModificadoPorUsuario { get; set; }
}