namespace RCD.Mob.GuardiaRelevo.Domain.Entities;

public class ConfigTurno
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!; // 'Matutino', 'Vespertino'
    public TimeSpan HoraInicioVentana { get; set; }
    public TimeSpan HoraFinVentana { get; set; }
    public bool HabilitadoEntrega { get; set; }
    public bool HabilitadoVerif { get; set; }
    public bool Activo { get; set; }
    public DateTime? FechaModificacion { get; set; }
}