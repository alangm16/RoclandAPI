namespace RCD.SuperAdmin.Application.DTOs.Seguridad;

public record SesionActivaDto(
    int Id,
    string Username,
    string? ProyectoCodigo,
    string Plataforma,
    string TokenReducido,
    DateTime FechaExpiracion,
    DateTime FechaCreacion,
    string? IpCreacion,
    bool Revocado
);

public record FiltroSesionesDto(
    int? UsuarioId,
    int? ProyectoId,
    int Pagina = 1,
    int TamanoPagina = 10
);