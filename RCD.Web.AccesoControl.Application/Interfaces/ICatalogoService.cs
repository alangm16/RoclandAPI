using RCD.Web.AccesoControl.Application.DTOs;

namespace RCD.Web.AccesoControl.Application.Interfaces;

public interface ICatalogoService
{
    Task<IEnumerable<CatalogoDto>> ObtenerAreasAsync();
    Task<IEnumerable<CatalogoDto>> ObtenerTiposIdentificacionAsync();
    Task<IEnumerable<CatalogoDto>> ObtenerMotivosVisitaAsync();
}