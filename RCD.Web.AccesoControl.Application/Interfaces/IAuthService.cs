using RCD.Web.AccesoControl.Application.DTOs;

namespace RCD.Web.AccesoControl.Application.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Obtiene el Perfil de AccesoControl correspondiente a un Usuario de SuperAdmin.
        /// Devuelve null si el usuario no tiene perfil en este módulo o está inactivo.
        /// </summary>
        Task<PerfilDto?> ObtenerPerfilPorSuperAdminIdAsync(int superAdminUsuarioId);

        /// <summary>
        /// Valida si el Perfil tiene el rol adecuado para ejecutar una acción (Opcional, según tu lógica).
        /// </summary>
        Task<bool> TienePermisoAsync(int superAdminUsuarioId, string tipoPerfilRequerido);
    }
}