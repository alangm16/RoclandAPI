using Microsoft.Extensions.Configuration;
using RCD.Mob.GuardiaRelevo.Application.DTOs;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using RCD.Mob.GuardiaRelevo.Application.Rondines;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using RCD.Mob.GuardiaRelevo.Domain.Interfaces;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Services;

public class RondinService : IRondinService
{
    private readonly IRondinRepository _rondines;
    private readonly IUsuarioRepository _usuarios;
    private readonly IConfiguration _config;

    public RondinService(
        IRondinRepository rondines,
        IUsuarioRepository usuarios,
        IConfiguration config)
    {
        _rondines = rondines;
        _usuarios = usuarios;
        _config = config;
    }

    // ─────────────────────────────────────────────
    // Obtener rondín activo según ventana horaria
    // ─────────────────────────────────────────────
    public async Task<RondinActivoDto?> ObtenerActivoAsync(CancellationToken ct = default)
    {
        var ahora = DateTime.Now;
        var tolerancia = int.Parse(_config["GuardiaRelevo:VentanaToleranciaMinutos"] ?? "30");
        var horaActual = TimeOnly.FromDateTime(ahora);

        var horaMatutino = new TimeOnly(7, 0);
        var horaNocturno = new TimeOnly(19, 0);

        var enVentanaMatutino = horaActual >= horaMatutino.AddMinutes(-tolerancia)
                             && horaActual <= horaMatutino.AddMinutes(tolerancia);

        var enVentanaNocturno = horaActual >= horaNocturno.AddMinutes(-tolerancia)
                             && horaActual <= horaNocturno.AddMinutes(tolerancia);

        if (!enVentanaMatutino && !enVentanaNocturno)
            return null;

        var turno = enVentanaMatutino ? "Matutino" : "Nocturno";
        var fecha = DateOnly.FromDateTime(ahora);
        var rondin = await _rondines.ObtenerActivoAsync(fecha, turno, ct);

        if (rondin is null) return null;

        return new RondinActivoDto(
            rondin.Id,
            rondin.Turno,
            rondin.HoraInicio.ToString("hh:mm tt"),
            rondin.Estado,
            new GuardiaDto(rondin.GuardiaSaliente!.Id, rondin.GuardiaSaliente.NombreCompleto),
            new GuardiaDto(rondin.GuardiaEntrante!.Id, rondin.GuardiaEntrante.NombreCompleto)
        );
    }

    // ─────────────────────────────────────────────
    // Validar QR de guardia saliente o entrante
    // ─────────────────────────────────────────────
    public async Task<ValidarQRResultDto> ValidarQRAsync(ValidarQRRequestDto request, CancellationToken ct = default)
    {
        var rondin = await _rondines.ObtenerPorIdAsync(request.RondinId, ct);

        if (rondin is null)
            return new(false, "Rondín no encontrado.");

        if (rondin.Estado is "Completado" or "Cancelado")
            return new(false, "El rondín ya fue cerrado.");

        var usuario = await _usuarios.ObtenerPorQRAsync(request.QRCode, ct);

        if (usuario is null)
            return new(false, "Código QR no reconocido.");

        var idEsperado = request.TipoGuardia == "Saliente"
            ? rondin.GuardiaSalienteId
            : rondin.GuardiaEntranteId;

        if (usuario.Id != idEsperado)
            return new(false, $"El QR no corresponde al guardia {request.TipoGuardia.ToLower()}.");

        var yaEscaneo = rondin.Eventos.Any(e =>
            e.TipoEvento == "QR" &&
            e.TipoGuardia == request.TipoGuardia &&
            e.UsuarioId == usuario.Id &&
            e.Exitoso);

        if (yaEscaneo)
            return new(false, $"El guardia {request.TipoGuardia.ToLower()} ya escaneó su QR.");

        await _rondines.RegistrarEventoAsync(new RondinEvento
        {
            RondinId = request.RondinId,
            UsuarioId = usuario.Id,
            TipoEvento = "QR",
            TipoGuardia = request.TipoGuardia,
            Exitoso = true,
            FechaEvento = DateTime.Now
        }, ct);

        // Si ambos QR ya están registrados → pasar a EnCurso
        var tieneSaliente = rondin.Eventos.Any(e => e.TipoEvento == "QR" && e.TipoGuardia == "Saliente" && e.Exitoso);
        var tieneEntrante = rondin.Eventos.Any(e => e.TipoEvento == "QR" && e.TipoGuardia == "Entrante" && e.Exitoso);

        // El evento recién guardado aún no está en la colección en memoria, lo contamos manualmente
        var saliente = tieneSaliente || request.TipoGuardia == "Saliente";
        var entrante = tieneEntrante || request.TipoGuardia == "Entrante";

        if (saliente && entrante)
            await _rondines.ActualizarEstadoAsync(request.RondinId, "EnCurso", ct);

        return new(true, $"QR del guardia {request.TipoGuardia.ToLower()} validado.", usuario.Id);
    }

    // ─────────────────────────────────────────────
    // Registrar firma y cerrar rondín si ambos firmaron
    // ─────────────────────────────────────────────
    public async Task<bool> FirmarAsync(FirmarRondinRequestDto request, CancellationToken ct = default)
    {
        var rondin = await _rondines.ObtenerPorIdAsync(request.RondinId, ct);

        if (rondin is null || rondin.Estado != "EnCurso")
            return false;

        var yaFirmo = rondin.Eventos.Any(e =>
            e.TipoEvento == "Firma" &&
            e.TipoGuardia == request.TipoGuardia &&
            e.UsuarioId == request.UsuarioId &&
            e.Exitoso);

        if (yaFirmo) return false;

        await _rondines.RegistrarEventoAsync(new RondinEvento
        {
            RondinId = request.RondinId,
            UsuarioId = request.UsuarioId,
            TipoEvento = "Firma",
            TipoGuardia = request.TipoGuardia,
            FirmaBase64 = request.FirmaBase64,
            Exitoso = true,
            FechaEvento = DateTime.Now
        }, ct);

        // Verificar si ambos firmaron para cerrar el rondín
        var firmaSaliente = rondin.Eventos.Any(e => e.TipoEvento == "Firma" && e.TipoGuardia == "Saliente" && e.Exitoso);
        var firmaEntrante = rondin.Eventos.Any(e => e.TipoEvento == "Firma" && e.TipoGuardia == "Entrante" && e.Exitoso);

        var saliente = firmaSaliente || request.TipoGuardia == "Saliente";
        var entrante = firmaEntrante || request.TipoGuardia == "Entrante";

        if (saliente && entrante)
        {
            await _rondines.ActualizarEstadoAsync(request.RondinId, "Completado", ct);
            await _rondines.ActualizarFechaFinAsync(request.RondinId, DateTime.Now, ct);
        }

        return true;
    }
}