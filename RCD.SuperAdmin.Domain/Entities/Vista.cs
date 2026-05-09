namespace RCD.SuperAdmin.Domain.Entities;
public class Vista
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Ruta { get; set; } = null!;
    public string? Icono { get; set; }
    public string? Descripcion { get; set; }
    public int Orden { get; set; } = 0;
    public bool Activo { get; set; } = true;
    public Proyecto Proyecto { get; set; } = null!;
    public ICollection<UsuarioVistaAcceso> AccesosUsuario { get; set; } = [];
}