namespace RCD.SuperAdmin.Application.DTOs.Alertas;

public record AlertaDto(
    int Id,
    int? ProyectoId,
    string? ProyectoCodigo,
    string Tipo,
    string Titulo,
    string Mensaje,
    DateTime Fecha,
    bool Resuelta,
    string? AccionUrl
);

public record FiltroAlertasDto(
    int? ProyectoId,
    string? Tipo,
    bool? Resuelta,
    DateTime? Desde,
    DateTime? Hasta,
    int Pagina = 1,
    int TamanoPagina = 10
);