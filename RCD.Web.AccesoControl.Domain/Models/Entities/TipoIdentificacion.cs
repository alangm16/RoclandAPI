using RCD.Web.AccesoControl.Domain.Models.Entities;

namespace RCD.Web.AccesoControl.Domain.Models.Entities;

public class TipoIdentificacion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; } = true;
    public ICollection<Persona> Personas { get; set; } = [];
}