
namespace RCD.Mob.GuardiaRelevo.Application.DTOs
{
    public record LoginRequestDto(string Usuario, string Password);

    public record LoginResponseDto(
        int Id,
        string NombreCompleto,
        string Usuario,
        string Rol,
        string QRCode,
        string Token,
        DateTime Expiracion
    );

    // Rondines
    public record RondinActivoDto(
        int Id,
        string Turno,
        string HoraInicio,
        string Estado,
        GuardiaDto GuardiaSaliente,
        GuardiaDto GuardiaEntrante
    );

    public record GuardiaDto(int Id, string NombreCompleto);

    public record ValidarQRRequestDto(
        int RondinId,
        string QRCode,
        string TipoGuardia   // "Saliente" | "Entrante"
    );

    public record FirmarRondinRequestDto(
        int RondinId,
        int UsuarioId,
        string TipoGuardia,
        string FirmaBase64
    );

    // Checklist
    public record ChecklistCategoriaDto(
        string Categoria,
        List<ChecklistPuntoDto> Puntos
    );

    public record ChecklistPuntoDto(
        int Id,
        string Nombre,
        string Descripcion,
        int Orden
    );

    public record GuardarRespuestasRequestDto(
        int RondinId,
        List<RespuestaItemDto> Respuestas,
        string? NotasFinales
    );

    public record RespuestaItemDto(
        int PuntoId,
        bool Respuesta,
        string? Comentario
    );
}
