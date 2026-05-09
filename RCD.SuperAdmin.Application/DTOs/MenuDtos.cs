namespace RCD.SuperAdmin.Application.DTOs.Menu;

public record VistaMenuDto(
    int Id,
    string Codigo,
    string Nombre,
    string Ruta,
    string? Icono,
    int Orden
);