// RCD.SuperAdmin.Application/DTOs/Proyectos/CrearProyectoRequest.cs
namespace RCD.SuperAdmin.Application.DTOs.Proyectos;

public record CrearProyectoRequest(
    string Codigo,
    string Nombre,
    string Plataforma,
    string? UrlBase,
    string? IconoCss,
    int Orden = 0
);