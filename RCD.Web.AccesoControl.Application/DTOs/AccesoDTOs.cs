using System.ComponentModel.DataAnnotations;

namespace RCD.Web.AccesoControl.Application.DTOs
{
    public record SolicitudPendienteResponse(
        int SolicitudId,
        int RegistroId,
        string TipoRegistro,
        int PersonaId,
        string NombrePersona,
        string? Empresa,
        string NumeroIdentificacion,
        string TipoID,
        string Motivo,
        string Area,
        DateTime FechaSolicitud,
        string? Placas
    );

    public record AccesoActivoResponse(
        int RegistroId,
        string TipoRegistro,
        string NombrePersona,
        string? Empresa,
        string NumeroGafete,
        DateTime FechaEntrada,
        string Area,
        double MinutosLlevaDentro
    );

    public record GafeteDisponibleResponse(
        int Id,
        string Codigo
    );

    public record AprobarSolicitudRequest(
        [Required, Range(1, int.MaxValue)] int SolicitudId,
        [Required, Range(1, int.MaxValue)] int GafeteId
    );

    public record RechazarSolicitudRequest(
        [Required, Range(1, int.MaxValue)] int SolicitudId,
        [StringLength(500)] string? Motivo // <- Le quitamos Required, el MinimumLength y le agregamos el '?'
    );

    public record MarcarSalidaRequest(
        [Required, Range(1, int.MaxValue)] int RegistroId,
        [Required] string TipoRegistro // "Visitante" o "Proveedor"
    );
}