namespace RCD.SuperAdmin.Application.DTOs.Delegaciones;

public record DelegacionDto(
    string OtorgadoPor,
    string OtorgadoA,
    string ProyectoCodigo,
    string ProyectoNombre,
    string Rol,
    DateTime FechaAsignacion
);

public record FiltroDelegacionesDto(
    int? ProyectoId,
    int Pagina = 1,
    int TamanoPagina = 10
);