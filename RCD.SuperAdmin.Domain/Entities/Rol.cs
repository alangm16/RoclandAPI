namespace RCD.SuperAdmin.Domain.Entities;

public class Rol
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string Nombre { get; set; } = null!;
    public int Nivel { get; set; } = 99;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public Proyecto Proyecto { get; set; } = null!;
    public ICollection<ProyectoUsuarioRol> UsuariosAsignados { get; set; } = [];
}