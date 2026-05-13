namespace RCD.SuperAdmin.Application.DTOs.Seguridad;

public record LogAccesoDto(
    int Id,
    string UsernameUsado,
    string? NombreCompleto,
    string? ProyectoCodigo,
    bool Exitoso,
    string? IpAddress,
    string? Plataforma,
    string? Detalle,
    DateTime Fecha
);

public record FiltroLogsDto(
    string? Username,
    string? ProyectoCodigo,
    DateTime? Desde,
    DateTime? Hasta,
    string? Plataforma,
    bool? Exitoso,
    int Pagina = 1,
    int TamanoPagina = 10
);