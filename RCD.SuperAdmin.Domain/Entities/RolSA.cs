namespace RCD.SuperAdmin.Domain.Entities;
public class RolSA
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int Nivel { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public ICollection<Usuario> Usuarios { get; set; } = [];
}