namespace RCD.SuperAdmin.Application.DTOs.Visibilidad;

public record VistaAccesoUsuarioDto(
    int VistaId,
    string Codigo,
    string Nombre,
    string Ruta,
    int Orden,
    bool TieneAcceso
);