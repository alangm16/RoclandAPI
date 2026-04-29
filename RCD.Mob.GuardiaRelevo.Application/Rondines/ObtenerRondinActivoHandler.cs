using MediatR;
using RCD.Mob.GuardiaRelevo.Application.DTOs;
using RCD.Mob.GuardiaRelevo.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace RCD.Mob.GuardiaRelevo.Application.Rondines;

public class ObtenerRondinActivoHandler : IRequestHandler<ObtenerRondinActivoQuery, RondinActivoDto?>
{
    private readonly IRondinRepository _rondines;
    private readonly IConfiguration _config;

    public ObtenerRondinActivoHandler(IRondinRepository rondines, IConfiguration config)
    {
        _rondines = rondines;
        _config = config;
    }

    public async Task<RondinActivoDto?> Handle(ObtenerRondinActivoQuery request, CancellationToken ct)
    {
        var ahora = DateTime.Now;
        var fecha = DateOnly.FromDateTime(ahora);
        var turno = ahora.Hour >= 7 && ahora.Hour < 19 ? "Matutino" : "Nocturno";
        var tolerancia = int.Parse(_config["GuardiaRelevo:VentanaToleranciaMinutos"] ?? "30");

        // Hora de corte del turno ± tolerancia
        var horaMatutino = new TimeOnly(7, 0);
        var horaNocturno = new TimeOnly(19, 0);
        var horaActual = TimeOnly.FromDateTime(ahora);

        var enVentanaMatutino = horaActual >= horaMatutino.AddMinutes(-tolerancia)
                              && horaActual <= horaMatutino.AddMinutes(tolerancia);
        var enVentanaNocturno = horaActual >= horaNocturno.AddMinutes(-tolerancia)
                              && horaActual <= horaNocturno.AddMinutes(tolerancia);

        if (!enVentanaMatutino && !enVentanaNocturno)
            return null; // Fuera de ventana horaria

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
}