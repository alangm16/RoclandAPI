// RCD.SuperAdmin.Application/DTOs/Proyectos/ProyectoDetalleDto.cs
namespace RCD.SuperAdmin.Application.DTOs.Proyectos;

public record VistaDetalleDto(
    int Id,
    string Codigo,
    string Nombre,
    string? Icono,
    int Orden
);

public record ProyectoDetalleDto(
    int Id,
    string Codigo,
    string Nombre,
    string Plataforma,
    string? UrlBase,
    string? IconoCss,
    int Orden,
    bool Activo,
    IEnumerable<VistaDetalleDto> Vistas
);

public record ProyectoPermitidoDto(
    int Id,
    string Codigo,
    string Nombre,
    string Plataforma,
    string? UrlBase,
    string? IconoCss,
    bool AccesoTotal,
    bool PuedeLeer,
    bool PuedeCrear,
    bool PuedeEditar,
    bool PuedeBorrar,
    IEnumerable<VistaPermitidaDto> Vistas
);