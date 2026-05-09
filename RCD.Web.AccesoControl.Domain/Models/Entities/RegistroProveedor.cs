using System.ComponentModel.DataAnnotations.Schema;

namespace RCD.Web.AccesoControl.Domain.Models.Entities;
public class RegistroProveedor
{
    public int Id { get; set; }
    public int PersonaId { get; set; }
    public int MotivoId { get; set; }
    public DateTime FechaEntrada { get; set; }
    public int PerfilEntradaId { get; set; }
    public DateTime? FechaSalida { get; set; }
    public string? UnidadPlacas { get; set; } = string.Empty;
    public string? FacturaRemision { get; set; } = string.Empty;
    public int? GafeteId { get; set; }
    public int? PerfilSalidaId { get; set; }
    public string EstadoAcceso { get; set; } = "Pendiente";
    public bool? ConsentimientoFirmado { get; set; }
    public string? Observaciones { get; set; }
    public string? IPSolicitud { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public int? CreadoPor { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public TimeSpan? HoraEntrada { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public TimeSpan? HoraSalida { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public int? MinutosEstancia { get; set; }

    public Persona Persona { get; set; } = null!;
    public MotivoVisita Motivo { get; set; } = null!;
    public Perfil PerfilEntrada { get; set; } = null!;
    public Perfil? PerfilSalida { get; set; }
    public Gafete? Gafete { get; set; }
}