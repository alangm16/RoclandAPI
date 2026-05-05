using RCD.Mob.GuardiaRelevo.Application.DTOs;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using RCD.Mob.GuardiaRelevo.Application.Rondines;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using RCD.Mob.GuardiaRelevo.Domain.Interfaces;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Services;

public class RondinService : IRondinService
{
    // Usamos los repositorios y servicios actualizados
    private readonly IRelevoRepository _relevos;
    private readonly IAuthService _auth;
    // Si necesitas actualizar estados del rondín individual, inyectamos su repo
    private readonly IRondinRepository _rondinesLocales;

    public RondinService(
        IRelevoRepository relevos,
        IAuthService auth,
        IRondinRepository rondinesLocales)
    {
        _relevos = relevos;
        _auth = auth;
        _rondinesLocales = rondinesLocales;
    }

    // ─────────────────────────────────────────────
    // 1. Obtener relevo activo según ventana horaria
    // ─────────────────────────────────────────────
    public async Task<RondinActivoDto?> ObtenerActivoAsync(CancellationToken ct = default)
    {
        var horaActual = TimeOnly.FromDateTime(DateTime.Now);
        var fechaActual = DateTime.Now.Date;

        // Leemos la configuración de turnos directamente de la base de datos
        var turnoActual = await _relevos.ObtenerConfigTurnoPorHoraAsync(horaActual, ct);
        if (turnoActual == null || !turnoActual.Activo)
            return null;

        var relevo = await _relevos.ObtenerPorTurnoYFechaAsync(turnoActual.Id, fechaActual, ct);
        if (relevo is null) return null;

        return new RondinActivoDto(
            relevo.Id,
            turnoActual.Nombre,
            turnoActual.HoraInicioVentana.ToString(@"hh\:mm"),
            relevo.Estado,
            relevo.GuardiaSaliente != null
                ? new GuardiaDto(relevo.GuardiaSaliente.SuperAdminUsuarioId, relevo.GuardiaSaliente.NumeroEmpleado) : null,
            relevo.GuardiaEntrante != null
                ? new GuardiaDto(relevo.GuardiaEntrante.SuperAdminUsuarioId, relevo.GuardiaEntrante.NumeroEmpleado) : null
        );
    }

    // ─────────────────────────────────────────────
    // 2. Validar QR (Delega la búsqueda a SuperAdmin)
    // ─────────────────────────────────────────────
    public async Task<ValidarQRResultDto> ValidarQRAsync(ValidarQRRequestDto request, CancellationToken ct = default)
    {
        var relevo = await _relevos.ObtenerPorIdAsync(request.RondinId, ct); // request.RondinId ahora trae el RelevoId

        if (relevo is null) return new(false, "Relevo no encontrado.");
        if (relevo.Estado is "Completado" or "Incompleto") return new(false, "El relevo ya fue cerrado.");

        var usuarioQRId = await _auth.ObtenerSuperAdminIdPorQRAsync(request.QRCode);
        if (usuarioQRId == null) return new(false, "Código QR no reconocido.");

        var idEsperado = request.TipoGuardia == "Saliente" ? relevo.GuardiaSalienteId : relevo.GuardiaEntranteId;
        if (usuarioQRId != idEsperado) return new(false, $"El QR no corresponde al guardia {request.TipoGuardia.ToLower()}.");

        if (relevo.Estado == "Pendiente")
        {
            await _relevos.ActualizarEstadoAsync(relevo.Id, "EnCurso", ct);
        }

        return new(true, $"QR validado.", usuarioQRId.Value);
    }

    // ─────────────────────────────────────────────
    // 3. Finalizar Rondín (Reemplaza a "FirmarAsync")
    // ─────────────────────────────────────────────
    public async Task<bool> FinalizarRondinAsync(int rondinId, CancellationToken ct = default)
    {
        // Buscamos el Rondín individual (la parte que hizo un solo guardia)
        var rondin = await _rondinesLocales.ObtenerPorIdAsync(rondinId, ct);
        if (rondin is null || rondin.Estado != "EnCurso") return false;

        // Actualizamos el Rondín a Completado
        rondin.Estado = "Completado";
        rondin.FechaFin = DateTime.Now;
        await _rondinesLocales.ActualizarAsync(rondin, ct);

        // Verificamos si el Relevo padre ya puede cerrarse
        var relevo = await _relevos.ObtenerPorIdAsync(rondin.RelevodId, ct);
        if (relevo != null)
        {
            // Si el Relevo tiene 2 rondines y ambos están Completados
            var todosCompletos = relevo.Rondines.Count == 2 && relevo.Rondines.All(r => r.Estado == "Completado");
            if (todosCompletos)
            {
                await _relevos.ActualizarEstadoAsync(relevo.Id, "Completado", ct);
            }
        }

        return true;
    }
}