using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Domain.Models.Entities;

namespace RCD.Web.AccesoControl.Application.Interfaces;

public interface IAccesoService
{
    Task<PersonaBusquedaResponse?> BuscarPersonaAsync(string numId);
    Task<VisitanteResponse> RegistrarVisitanteAsync(CrearVisitanteRequest req, string ip);
    Task<ProveedorResponse> RegistrarProveedorAsync(CrearProveedorRequest req, string ip);
    Task<IEnumerable<SolicitudPendienteResponse>> ObtenerSolicitudesPendientesAsync();
    Task<SolicitudPendienteResponse?> ObtenerSolicitudPorIdAsync(int solicitudId);
    Task<IEnumerable<AccesoActivoResponse>> ObtenerAccesosActivosAsync();
    Task<IEnumerable<AccesoActivoResponse>> ObtenerAccesosActivosZonaAsync();
    Task<bool> AprobarSolicitudAsync(AprobarSolicitudRequest request);
    Task<bool> RechazarSolicitudAsync(RechazarSolicitudRequest request);
    Task<bool> MarcarSalidaAsync(MarcarSalidaRequest request);
    Task<bool> GuardarFcmTokenAsync(int guardiaId, string fcmToken);
    Task<IEnumerable<GafeteDisponibleResponse>> ObtenerGafetesDisponiblesAsync();
}
