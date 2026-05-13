namespace RCD.SuperAdmin.Application.DTOs.Auditoria;

public record AuditoriaDto(
    string EntidadAfectada,
    string NombreEntidad,
    int? RegistroId,
    string Accion,
    string UsuarioResponsable,
    DateTime Fecha
);

public record FiltroAuditoriaDto(
    string? Usuario,
    string? Entidad,
    string? Accion,
    DateTime? Desde,
    DateTime? Hasta,
    int Pagina = 1,
    int TamanoPagina = 10
);