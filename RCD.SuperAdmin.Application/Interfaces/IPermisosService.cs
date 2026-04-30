// IPermisosService.cs
using RCD.SuperAdmin.Application.DTOs.Auth;      
using RCD.SuperAdmin.Application.DTOs.Permisos; 

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IPermisosService
{
    Task<MatrizPermisosDto> ObtenerMatrizRolAsync(int rolId);
    Task<MatrizPermisosDto> ObtenerMatrizUsuarioAsync(int usuarioId);

    Task UpsertPermisoRolAsync(AsignarPermisoRolRequest request);
    Task UpsertPermisoUsuarioAsync(AsignarPermisoUsuarioRequest request);

    Task RevocarPermisoRolAsync(int rolId, int proyectoId, int? vistaId);
    Task RevocarPermisoUsuarioAsync(int usuarioId, int proyectoId, int? vistaId);

    Task<IEnumerable<ProyectoPermitidoDto>> ResolverPermisosEfectivosAsync(int usuarioId);
}