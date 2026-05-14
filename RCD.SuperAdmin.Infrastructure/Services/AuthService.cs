using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.Auth;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Infrastructure.Data;
using RCD.Shared.Kernel.Settings;
using Microsoft.Extensions.Options;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class AuthService(
    SuperAdminDbContext db,
    IJwtService jwtService,
    IOptions<JwtSettings> jwtSettings) : IAuthService
{
    private readonly JwtSettings _jwt = jwtSettings.Value;

    public async Task<AuthResultDto> LoginDirectoAsync(LoginDirectoDto dto)
    {
        // 1. Buscar usuario activo
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Username == dto.Username && u.Activo);

        // 2. Validar existencia + bloqueo + contraseña
        await ValidarCredencialesAsync(usuario, dto.Password, dto.CodigoProyecto);

        // 3. Verificar que el usuario tenga un rol activo en el proyecto solicitado
        var asignacion = await db.ProyectoUsuarioRoles
            .Include(pur => pur.Proyecto)
            .Include(pur => pur.Rol)
            .FirstOrDefaultAsync(pur =>
                pur.UsuarioId == usuario!.Id &&
                pur.Proyecto.Codigo == dto.CodigoProyecto &&
                pur.Activo == true &&
                pur.Proyecto.Activo == true);

        if (asignacion is null)
            throw new UnauthorizedAccessException(
                $"El usuario '{dto.Username}' no tiene acceso al proyecto '{dto.CodigoProyecto}'.");

        // 4. Generar tokens
        var tokenClaims = new TokenDirectoClaimsDto(
            UsuarioId: usuario!.Id,
            Username: usuario.Username,
            ProyectoId: asignacion.ProyectoId,
            CodigoProyecto: asignacion.Proyecto.Codigo,
            RolId: asignacion.RolId,
            NombreRol: asignacion.Rol.Nombre,
            NivelRol: asignacion.Rol.Nivel,
            Plataforma: dto.Plataforma
        );

        var accessToken = jwtService.GenerarTokenDirecto(tokenClaims);
        var refreshToken = jwtService.GenerarRefreshToken();
        var expiracion = DateTime.UtcNow.AddMinutes(_jwt.ExpirationMinutes);

        // 5. Persistir RefreshToken
        await GuardarRefreshTokenAsync(
            usuario.Id, asignacion.ProyectoId, dto.Plataforma, refreshToken);

        // 6. Actualizar TokenDispositivo (FCM / device)
        await ActualizarTokenDispositivoAsync(
            usuario.Id, asignacion.ProyectoId, dto.Plataforma);

        // 7. Resetear intentos fallidos + UltimoAcceso
        await RegistrarAccesoExitosoAsync(usuario, dto.CodigoProyecto, dto.Plataforma);

        return new AuthResultDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            Expiracion: expiracion,
            Usuario: MapUsuarioToken(usuario)
        );
    }

    public async Task<AuthMaestroResultDto> LoginMaestroAsync(LoginMaestroDto dto)
    {
        // 1. Buscar usuario activo (sin RolSA)
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Username == dto.Username && u.Activo);

        // 2. Validar credenciales (sin proyecto específico)
        await ValidarCredencialesAsync(usuario, dto.Password, proyectoCodigo: null);

        // 3. Obtener proyecto 'super-admin'
        var proyectoSA = await db.Proyectos
            .FirstOrDefaultAsync(p => p.Codigo == "super-admin" && p.Activo);
        if (proyectoSA == null)
            throw new Exception("Proyecto super-admin no está configurado.");

        // 4. Obtener la asignación activa del usuario al proyecto super-admin
        var asignacionSA = await db.ProyectoUsuarioRoles
            .Include(pur => pur.Rol)
            .FirstOrDefaultAsync(pur =>
                pur.UsuarioId == usuario.Id &&
                pur.ProyectoId == proyectoSA.Id &&
                pur.Activo);

        if (asignacionSA == null)
            throw new UnauthorizedAccessException("El usuario no tiene acceso al panel SuperAdmin.");

        // 5. Obtener todos los demás proyectos a los que tiene acceso (opcional)
        var proyectos = await db.ProyectoUsuarioRoles
            .Include(pur => pur.Proyecto)
            .Include(pur => pur.Rol)
            .Where(pur =>
                pur.UsuarioId == usuario.Id &&
                pur.Activo &&
                pur.Proyecto.Activo &&
                pur.ProyectoId != proyectoSA.Id) // excluir super-admin si no quieres duplicado
            .OrderBy(pur => pur.Proyecto.Orden)
            .ToListAsync();

        // 6. Generar token maestro con el rol obtenido del proyecto super-admin
        var tokenClaims = new TokenMaestroClaimsDto(
            UsuarioId: usuario.Id,
            Username: usuario.Username,
            Rol: asignacionSA.Rol.Nombre,      // ← ahora Rol
            Nivel: asignacionSA.Rol.Nivel,     // ← ahora Nivel
            Plataforma: dto.Plataforma
        );

        var accessToken = jwtService.GenerarTokenMaestro(tokenClaims);
        var refreshToken = jwtService.GenerarRefreshToken();
        var expiracion = DateTime.UtcNow.AddMinutes(_jwt.MaestroExpirationMinutes);

        // 7. Persistir RefreshToken (proyectoId = null → sesión del panel SA)
        await GuardarRefreshTokenAsync(usuario.Id, null, dto.Plataforma, refreshToken);

        // 8. Registrar acceso exitoso
        await RegistrarAccesoExitosoAsync(usuario, null, dto.Plataforma);

        // 9. Mapear usuario (sin RolSA)
        return new AuthMaestroResultDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            Expiracion: expiracion,
            Usuario: MapUsuarioToken(usuario),   // Este método ya no debe incluir RolSA
            ProyectosAccesibles: proyectos.Select(pur => new ProyectoAccesoDto(
                Id: pur.ProyectoId,
                Codigo: pur.Proyecto.Codigo,
                Nombre: pur.Proyecto.Nombre,
                Plataforma: pur.Proyecto.Plataforma,
                IconoCss: pur.Proyecto.IconoCss,
                UrlBase: pur.Proyecto.UrlBase,
                RolEnProyecto: pur.Rol.Nombre,
                NivelRol: pur.Rol.Nivel
            ))
        );
    }

    public async Task<AuthResultDto> LoginQrAsync(LoginQrDto dto)
    {
        // 1. Buscar usuario activo por su QRCode
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.QRCode == dto.QrCode && u.Activo);

        // Validaciones básicas de seguridad (similares a ValidarCredencialesAsync)
        if (usuario is null)
        {
            // Registramos el log de intento fallido sin revelar datos sensibles
            await RegistrarLogAsync(null, dto.CodigoProyecto, "QR Login", false, "Código QR no encontrado o usuario inactivo.");
            throw new UnauthorizedAccessException("Código QR inválido o usuario inactivo.");
        }

        if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta > DateTime.UtcNow)
        {
            await RegistrarLogAsync(usuario.Id, dto.CodigoProyecto, usuario.Username, false, $"Cuenta bloqueada hasta {usuario.BloqueadoHasta:HH:mm:ss} UTC.");
            throw new UnauthorizedAccessException($"Cuenta bloqueada temporalmente. Intente después de {usuario.BloqueadoHasta:HH:mm} UTC.");
        }

        // 2. Verificar que el usuario tenga un rol activo en el proyecto solicitado (Acceso Control Móvil)
        var asignacion = await db.ProyectoUsuarioRoles
            .Include(pur => pur.Proyecto)
            .Include(pur => pur.Rol)
            .FirstOrDefaultAsync(pur =>
                pur.UsuarioId == usuario.Id &&
                pur.Proyecto.Codigo == dto.CodigoProyecto &&
                pur.Activo == true &&
                pur.Proyecto.Activo == true);

        if (asignacion is null)
        {
            await RegistrarLogAsync(usuario.Id, dto.CodigoProyecto, usuario.Username, false, "Sin acceso al proyecto mediante QR.");
            throw new UnauthorizedAccessException($"El usuario '{usuario.Username}' no tiene acceso al proyecto '{dto.CodigoProyecto}'.");
        }

        // 3. Generar tokens (Token Directo)
        var tokenClaims = new TokenDirectoClaimsDto(
            UsuarioId: usuario.Id,
            Username: usuario.Username,
            ProyectoId: asignacion.ProyectoId,
            CodigoProyecto: asignacion.Proyecto.Codigo,
            RolId: asignacion.RolId,
            NombreRol: asignacion.Rol.Nombre,
            NivelRol: asignacion.Rol.Nivel,
            Plataforma: dto.Plataforma
        );

        var accessToken = jwtService.GenerarTokenDirecto(tokenClaims);
        var refreshToken = jwtService.GenerarRefreshToken();
        var expiracion = DateTime.UtcNow.AddMinutes(_jwt.ExpirationMinutes);

        // 4. Persistir RefreshToken
        await GuardarRefreshTokenAsync(usuario.Id, asignacion.ProyectoId, dto.Plataforma, refreshToken);

        // 5. Actualizar TokenDispositivo (FCM / device)
        await ActualizarTokenDispositivoAsync(usuario.Id, asignacion.ProyectoId, dto.Plataforma);

        // 6. Resetear intentos fallidos + UltimoAcceso y guardar Log
        await RegistrarAccesoExitosoAsync(usuario, dto.CodigoProyecto, dto.Plataforma);

        return new AuthResultDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            Expiracion: expiracion,
            Usuario: MapUsuarioToken(usuario)
        );
    }

    public async Task<AuthResultDto> RefrescarTokenAsync(RefreshTokenDto dto)
    {
        // 1. Buscar el RefreshToken vigente (sin incluir RolSA)
        var stored = await db.RefreshTokens
            .Include(rt => rt.Usuario)
            .Include(rt => rt.Proyecto)
            .FirstOrDefaultAsync(rt =>
                rt.Token == dto.RefreshToken &&
                rt.Plataforma == dto.Plataforma &&
                !rt.Revocado);

        if (stored is null)
            throw new UnauthorizedAccessException("RefreshToken inválido o revocado.");

        if (stored.FechaExpiracion < DateTime.UtcNow)
            throw new UnauthorizedAccessException("RefreshToken expirado.");

        var usuario = stored.Usuario;
        if (!usuario.Activo)
            throw new UnauthorizedAccessException("El usuario está inactivo.");

        // 2. Revocar el token usado
        stored.Revocado = true;
        db.RefreshTokens.Update(stored);

        // 3. Generar nuevos tokens
        string accessToken;
        DateTime expiracion;

        if (stored.ProyectoId is not null)
        {
            // Refresh de token directo (proyecto específico)
            var asignacion = await db.ProyectoUsuarioRoles
                .Include(pur => pur.Rol)
                .FirstOrDefaultAsync(pur =>
                    pur.UsuarioId == usuario.Id &&
                    pur.ProyectoId == stored.ProyectoId &&
                    pur.Activo);

            if (asignacion is null)
                throw new UnauthorizedAccessException(
                    "El usuario ya no tiene acceso al proyecto.");

            expiracion = DateTime.UtcNow.AddMinutes(_jwt.ExpirationMinutes);
            accessToken = jwtService.GenerarTokenDirecto(new TokenDirectoClaimsDto(
                UsuarioId: usuario.Id,
                Username: usuario.Username,
                ProyectoId: stored.ProyectoId.Value,
                CodigoProyecto: stored.Proyecto!.Codigo,
                RolId: asignacion.RolId,
                NombreRol: asignacion.Rol.Nombre,
                NivelRol: asignacion.Rol.Nivel,
                Plataforma: dto.Plataforma
            ));
        }
        else
        {
            // Refresh de token maestro → obtener rol desde proyecto super-admin
            var proyectoSA = await db.Proyectos
                .FirstOrDefaultAsync(p => p.Codigo == "super-admin" && p.Activo);
            if (proyectoSA == null)
                throw new Exception("Proyecto super-admin no está configurado.");

            var asignacionSA = await db.ProyectoUsuarioRoles
                .Include(pur => pur.Rol)
                .FirstOrDefaultAsync(pur =>
                    pur.UsuarioId == usuario.Id &&
                    pur.ProyectoId == proyectoSA.Id &&
                    pur.Activo);

            if (asignacionSA == null)
                throw new UnauthorizedAccessException(
                    "El usuario ya no tiene acceso al panel SuperAdmin.");

            expiracion = DateTime.UtcNow.AddMinutes(_jwt.MaestroExpirationMinutes);
            accessToken = jwtService.GenerarTokenMaestro(new TokenMaestroClaimsDto(
                UsuarioId: usuario.Id,
                Username: usuario.Username,
                Rol: asignacionSA.Rol.Nombre,
                Nivel: asignacionSA.Rol.Nivel,
                Plataforma: dto.Plataforma
            ));
        }

        // 4. Generar nuevo RefreshToken
        var nuevoRefresh = jwtService.GenerarRefreshToken();
        await GuardarRefreshTokenAsync(usuario.Id, stored.ProyectoId, dto.Plataforma, nuevoRefresh);

        await db.SaveChangesAsync();

        return new AuthResultDto(
            AccessToken: accessToken,
            RefreshToken: nuevoRefresh,
            Expiracion: expiracion,
            Usuario: MapUsuarioToken(usuario)   // sin RolSA
        );
    }

    public async Task LogoutAsync(int usuarioId, string plataforma, int? proyectoId)
    {
        // Revocar todos los RefreshTokens activos del usuario en esa plataforma/proyecto
        var tokens = await db.RefreshTokens
            .Where(rt =>
                rt.UsuarioId == usuarioId &&
                rt.Plataforma == plataforma &&
                rt.ProyectoId == proyectoId &&
                !rt.Revocado)
            .ToListAsync();

        foreach (var t in tokens)
            t.Revocado = true;

        await db.SaveChangesAsync();
    }

    private async Task ValidarCredencialesAsync(
        Usuario? usuario, string password, string? proyectoCodigo)
    {
        // Fallo: usuario no existe — no revelar si el username es válido
        if (usuario is null)
        {
            await RegistrarLogAsync(null, proyectoCodigo, "Desconocido",
                exitoso: false, "Usuario no encontrado.");
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        // Fallo: cuenta bloqueada temporalmente
        if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta > DateTime.UtcNow)
        {
            await RegistrarLogAsync(usuario.Id, proyectoCodigo, usuario.Username,
                exitoso: false,
                $"Cuenta bloqueada hasta {usuario.BloqueadoHasta:HH:mm:ss} UTC.");
            throw new UnauthorizedAccessException(
                $"Cuenta bloqueada temporalmente. Intente después de {usuario.BloqueadoHasta:HH:mm} UTC.");
        }

        // Fallo: contraseña incorrecta
        if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
        {
            usuario.IntentosFallidos++;

            // Bloquear tras 5 intentos — 15 minutos
            if (usuario.IntentosFallidos >= 5)
            {
                usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(15);
                usuario.IntentosFallidos = 0;
            }

            db.Usuarios.Update(usuario);
            await db.SaveChangesAsync();

            await RegistrarLogAsync(usuario.Id, proyectoCodigo, usuario.Username,
                exitoso: false,
                $"Contraseña incorrecta. Intento {usuario.IntentosFallidos}/5.");

            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }
    }

    private async Task GuardarRefreshTokenAsync(
        int usuarioId, int? proyectoId, string plataforma, string token)
    {
        db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuarioId,
            ProyectoId = proyectoId,
            Plataforma = plataforma,
            Token = token,
            FechaExpiracion = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            Revocado = false,
            FechaCreacion = DateTime.UtcNow
        });
    }

    private async Task ActualizarTokenDispositivoAsync(
        int usuarioId, int proyectoId, string plataforma)
    {
        var existing = await db.TokensDispositivo.FirstOrDefaultAsync(td =>
            td.UsuarioId == usuarioId &&
            td.ProyectoId == proyectoId &&
            td.Plataforma == plataforma);

        if (existing is null)
        {
            db.TokensDispositivo.Add(new TokenDispositivo
            {
                UsuarioId = usuarioId,
                ProyectoId = proyectoId,
                Plataforma = plataforma,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                FechaModificacion = DateTime.UtcNow
            });
        }
        else
        {
            existing.Activo = true;
            existing.FechaModificacion = DateTime.UtcNow;
            db.TokensDispositivo.Update(existing);
        }
    }

    private async Task RegistrarAccesoExitosoAsync(
        Usuario usuario, string? proyectoCodigo, string plataforma)
    {
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        usuario.UltimoAcceso = DateTime.UtcNow;
        db.Usuarios.Update(usuario);

        await RegistrarLogAsync(usuario.Id, proyectoCodigo, usuario.Username,
            exitoso: true, plataforma: plataforma);

        await db.SaveChangesAsync();
    }

    private async Task RegistrarLogAsync(
        int? usuarioId, string? proyectoCodigo,
        string usernameUsado, bool exitoso,
        string? detalle = null, string plataforma = "Web")
    {
        int? proyectoId = null;

        if (proyectoCodigo is not null)
        {
            proyectoId = await db.Proyectos
                .Where(p => p.Codigo == proyectoCodigo)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();
        }

        db.LogsAcceso.Add(new LogAcceso
        {
            UsuarioId = usuarioId,
            ProyectoId = proyectoId,
            UsernameUsado = usernameUsado,
            Exitoso = exitoso,
            Plataforma = plataforma,
            Detalle = detalle,
            Fecha = DateTime.UtcNow
        });

        // El log se guarda junto al SaveChangesAsync del flujo principal.
        // En fallos previos al SaveChanges (ej. contraseña incorrecta),
        // se hace SaveChangesAsync explícito dentro de ValidarCredencialesAsync.
    }

    private static UsuarioTokenDto MapUsuarioToken(Usuario u) => new(
        Id: u.Id,
        NombreCompleto: u.NombreCompleto,
        Username: u.Username,
        Email: u.Email
    );

    public async Task<IEnumerable<ProyectoAccesoDto>> DescubrirProyectosAsync(string username)
    {
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Username == username && u.Activo);

        if (usuario is null)
            return Array.Empty<ProyectoAccesoDto>();

        return await db.ProyectoUsuarioRoles
            .Include(pur => pur.Proyecto)
            .Include(pur => pur.Rol)
            .Where(pur =>
                pur.UsuarioId == usuario.Id &&
                pur.Activo &&
                pur.Proyecto.Activo)
            .OrderBy(pur => pur.Proyecto.Orden)
            .Select(pur => new ProyectoAccesoDto(
                pur.Proyecto.Id,
                pur.Proyecto.Codigo,
                pur.Proyecto.Nombre,
                pur.Proyecto.Plataforma,
                pur.Proyecto.IconoCss,
                pur.Proyecto.UrlBase,
                pur.Rol.Nombre,
                pur.Rol.Nivel
            ))
            .ToListAsync();
    }
}