// ProyectoPermitidoDto.cs
namespace RCD.SuperAdmin.Application.DTOs.Auth;

public record VistaPermitidaDto(
    int Id,
    string Codigo,
    string Nombre,
    string? Icono,
    bool PuedeLeer,
    bool PuedeCrear,
    bool PuedeEditar,
    bool PuedeBorrar
);

public record ProyectoPermitidoDto(
    int Id,
    string Codigo,
    string Nombre,
    string Plataforma,
    string? UrlBase,
    string? IconoCss,
    bool AccesoTotal,   // true = permiso a nivel proyecto sin restricción de vista
    bool PuedeLeer,
    bool PuedeCrear,
    bool PuedeEditar,
    bool PuedeBorrar,
    IEnumerable<VistaPermitidaDto> Vistas
);