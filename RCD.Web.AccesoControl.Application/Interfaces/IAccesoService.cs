using RCD.Web.AccesoControl.Application.DTOs;

namespace RCD.Web.AccesoControl.Application.Interfaces;

public interface IAccesoService
{
    Task<PersonaBusquedaResponse?> BuscarPersonaAsync(string numId);
    Task<VisitanteResponse> RegistrarVisitanteAsync(CrearVisitanteRequest req, int perfilEntradaId, string ip);
    Task<ProveedorResponse> RegistrarProveedorAsync(CrearProveedorRequest req, int perfilEntradaId, string ip);
    Task<IEnumerable<SolicitudPendienteResponse>> ObtenerSolicitudesPendientesAsync();
    Task<SolicitudPendienteResponse?> ObtenerSolicitudPorIdAsync(int solicitudId);
    Task<IEnumerable<AccesoActivoResponse>> ObtenerAccesosActivosAsync();
    Task<IEnumerable<AccesoActivoResponse>> ObtenerAccesosActivosZonaAsync();
    Task<bool> AprobarSolicitudAsync(AprobarSolicitudRequest request, int perfilId);
    Task<bool> RechazarSolicitudAsync(RechazarSolicitudRequest request, int perfilId);
    Task<bool> MarcarSalidaAsync(MarcarSalidaRequest request, int perfilSalidaId);
    Task<IEnumerable<GafeteDisponibleResponse>> ObtenerGafetesDisponiblesAsync();
}