using RCD.SuperAdmin.Application.DTOs;

namespace RCD.SuperAdmin.Application.Interfaces
{
    public interface IPermisosService
    {
        Task<MatrizPermisosDto> ObtenerMatrizPermisosAsync(int usuarioId);
        Task AsignarPermisoAsync (AsignarPermisoRequest request);
        Task RevocarPermisoAsync(int permisoId);
        Task<IEnumerable<ProyectoPermitidoDto>> ObtenerProyectosPermitidosAsync (int usuarioId);
    }
}
