using RCD.SuperAdmin.Application.DTOs.Visibilidad;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IVisibilidadService
{
    Task<IEnumerable<VistaAccesoUsuarioDto>> ObtenerVistasAccesoAsync(int usuarioId, int proyectoId);
    Task ActualizarVistaAccesoAsync(int usuarioId, int vistaId, bool tieneAcceso);
}