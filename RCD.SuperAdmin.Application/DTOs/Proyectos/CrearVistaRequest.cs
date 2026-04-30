// RCD.SuperAdmin.Application/DTOs/Proyectos/CrearVistaRequest.cs
namespace RCD.SuperAdmin.Application.DTOs.Proyectos;

public record CrearVistaRequest(
    string Codigo,
    string Nombre,
    string? Icono,
    int Orden = 0
);