namespace RCD.Web.AccesoControl.Domain.Models.Entities
{
    public class RegistroVisitante
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public int AreaId { get; set; }
        public int MotivoId { get; set; }
        public DateTime FechaEntrada { get; set; }
        public TimeSpan? HoraEntrada { get; private set; } // Campo calculado en BD
        public DateTime? FechaSalida { get; set; }
        public TimeSpan? HoraSalida { get; private set; } // Campo calculado en BD
        public int? MinutosEstancia { get; private set; } // Campo calculado en BD
        public int? GafeteId { get; set; }

        // FKs actualizadas a Perfil
        public int PerfilEntradaId { get; set; }
        public int? PerfilSalidaId { get; set; }

        public string EstadoAcceso { get; set; } = "Pendiente";
        public bool? ConsentimientoFirmado { get; set; } = false;
        public string? Observaciones { get; set; }
        public string? IPSolicitud { get; set; }
        public DateTime? FechaCreacion { get; set; } = DateTime.Now;

        // Propiedades de navegación
        public Persona Persona { get; set; } = null!;
        public Area Area { get; set; } = null!;
        public MotivoVisita Motivo { get; set; } = null!;
        public Gafete? Gafete { get; set; }
        public Perfil PerfilEntrada { get; set; } = null!;
        public Perfil? PerfilSalida { get; set; }
    }
}