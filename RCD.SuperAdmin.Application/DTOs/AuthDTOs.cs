
namespace RCD.SuperAdmin.Application.DTOs
{
    public record LoginRequest(string Username, string Password);

    public record ProyectoPermitidoDto(
        int Id,
        string Codigo,
        string Nombre,
        string Plataforma,
        string? UrlBase,
        string? IconoCss,
        IEnumerable<VistaPermitidaDto> Vistas
    );

    public record VistaPermitidaDto(int Id, string Codigo, string Nombre, string? Icono);

    public record LoginResponse(
        string Token,
        string NombreCompleto,
        string Username,
        IEnumerable<string> Roles,
        IEnumerable<ProyectoPermitidoDto> ProyectosPermitidos
    );
}
