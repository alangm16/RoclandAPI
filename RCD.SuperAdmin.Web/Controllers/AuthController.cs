// RCD.SuperAdmin.Web/Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.SuperAdmin.Application.DTOs.Auth;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.Shared.Kernel.Interfaces;

namespace RCD.SuperAdmin.Web.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
[ApiExplorerSettings(GroupName = "superadmin")]
public class AuthController(
    IAuthService authService,
    ICurrentUserService currentUserService) : ControllerBase
{
    // ── FLUJO 1: LOGIN ESPECÍFICO (MÓVIL / DESKTOP) ──
    [HttpPost("login-directo")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginDirecto([FromBody] LoginDirectoDto request)
    {
        var ipAddress = ObtenerIpRealCliente();
        try
        {
            var result = await authService.LoginDirectoAsync(request, ipAddress);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { mensaje = ex.Message });
        }
    }

    // ── FLUJO 2: LOGIN MULTI-PROYECTO (PANEL WEB) ──
    [HttpPost("login-maestro")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginMaestro([FromBody] LoginMaestroDto request)
    {
        var ipAddress = ObtenerIpRealCliente();
        try
        {
            var result = await authService.LoginMaestroAsync(request, ipAddress);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Atrapamos la excepción del servicio y devolvemos un 401 limpio a Angular
            return Unauthorized(new { mensaje = ex.Message });
        }
    }

    // ── FLUJO 5: LOGIN POR CÓDIGO QR (MÓVIL) ──
    [HttpPost("login-qr")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginQr([FromBody] LoginQrDto request)
    {
        var ipAddress = ObtenerIpRealCliente();
        try
        {
            var result = await authService.LoginQrAsync(request, ipAddress);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { mensaje = ex.Message });
        }
    }

    // ── FLUJO 3: RENOVACIÓN DE TOKENS ──
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto request)
    {
        var ipAddress = ObtenerIpRealCliente();
        try
        {
            var result = await authService.RefrescarTokenAsync(request, ipAddress);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { mensaje = ex.Message });
        }
    }

    // ── FLUJO 4: CIERRE DE SESIÓN ──
    [HttpPost("logout")]
    [Authorize] // ¡Solo usuarios logueados pueden hacer logout!
    public async Task<IActionResult> Logout()
    {
        // Extraemos los datos 100% confiables desde el Token JWT
        var userId = currentUserService.GetUserId();
        if (userId is null)
            return Unauthorized();

        var plataforma = currentUserService.GetPlataforma();
        var proyectoId = currentUserService.GetProyectoId();

        // Mandamos revocar los tokens a la base de datos
        await authService.LogoutAsync(userId.Value, plataforma, proyectoId);

        return NoContent(); // 204: Todo salió bien, no hay contenido que devolver
    }

    [HttpGet("proyectos")]
    [AllowAnonymous]
    public async Task<IActionResult> DescubrirProyectos([FromQuery] string username)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return Ok(Array.Empty<ProyectoAccesoDto>());

        var proyectos = await authService.DescubrirProyectosAsync(username);
        return Ok(proyectos);
    }

    private string ObtenerIpRealCliente()
    {
        // 1. Intentar leer la cabecera X-Forwarded-For (Estándar en proxies)
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For puede contener múltiples IPs (cliente, proxy1, proxy2). 
            // La primera es siempre la del cliente original.
            return forwardedFor.Split(',')[0].Trim();
        }

        // 2. Intentar leer X-Real-IP (Usado comúnmente por Nginx)
        var realIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // 3. Fallback a la IP de la conexión (si entra directo sin proxy)
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP desconocida";
    }
}