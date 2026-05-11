namespace RCD.Web.AccesoControl.Application.DTOs;

public record PerfilContextoDto(
    int PerfilId,
    int SuperAdminUsuarioId,
    string NombreCompleto,
    string NombreRol,      // viene del JWT (claim "nombreRol")
    int NivelRol,       // viene del JWT (claim "nivelRol")
    string? Turno,
    string? NumeroEmpleado
);