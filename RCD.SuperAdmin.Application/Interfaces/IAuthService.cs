using RCD.SuperAdmin.Application.DTOs.Auth;

namespace RCD.SuperAdmin.Application.Interfaces;

/// Contrato del IdP centralizado.
/// Cubre los tres flujos de autenticación: directo (móvil/desktop),
/// maestro (panel web multi-proyecto) y renovación de tokens.

public interface IAuthService
{
    /// Flujo 1 — App Móvil / Desktop (acceso directo a un proyecto).
    /// Valida usuario + contraseña + rol activo en el proyecto solicitado.
    /// Devuelve un JWT con claims [Usuario, Proyecto, Rol].
    Task<AuthResultDto> LoginDirectoAsync(LoginDirectoDto dto);

    /// Flujo 2 — Panel Web SuperAdmin (acceso multi-proyecto).
    /// Valida usuario + contraseña sin especificar proyecto.
    /// Devuelve un "token maestro" + la lista de proyectos a los que tiene acceso.
    Task<AuthMaestroResultDto> LoginMaestroAsync(LoginMaestroDto dto);

    /// Renueva un JWT expirado usando un RefreshToken válido.
    /// Aplica a ambos flujos.
    Task<AuthResultDto> RefrescarTokenAsync(RefreshTokenDto dto);

    /// Revoca todos los RefreshTokens activos del usuario en la plataforma indicada.
    Task LogoutAsync(int usuarioId, string plataforma, int? proyectoId);
}