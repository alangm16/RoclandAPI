using MediatR;
using RCD.Mob.GuardiaRelevo.Application.DTOs;
using RCD.Mob.GuardiaRelevo.Application.Interfaces; // Asegura que este sea tu namespace correcto

namespace RCD.Mob.GuardiaRelevo.Application.Rondines;

public class ObtenerRondinActivoHandler : IRequestHandler<ObtenerRondinActivoQuery, RondinActivoDto?>
{
    private readonly IRelevoRepository _relevos;

    // Eliminamos IConfiguration porque los horarios ahora viven en la Base de Datos
    public ObtenerRondinActivoHandler(IRelevoRepository relevos)
    {
        _relevos = relevos;
    }

    public async Task<RondinActivoDto?> Handle(ObtenerRondinActivoQuery request, CancellationToken ct)
    {
        var ahora = DateTime.Now;
        var horaActual = TimeOnly.FromDateTime(ahora);
        var fechaActual = ahora.Date;

        // 1. Preguntamos a la BD si la hora actual cae dentro de la ventana de algún turno configurado
        var turnoActual = await _relevos.ObtenerConfigTurnoPorHoraAsync(horaActual, ct);

        if (turnoActual == null || !turnoActual.Activo)
            return null; // Fuera de ventana horaria, el móvil no debería mostrar el botón de iniciar

        // 2. Buscamos si ya existe el Relevo maestro creado para hoy en este turno
        var relevo = await _relevos.ObtenerPorTurnoYFechaAsync(turnoActual.Id, fechaActual, ct);

        if (relevo is null)
            return null; // Aún no lo inician (se crearía en otro endpoint cuando escanean el QR)

        // 3. Mapeamos la respuesta usando NumeroEmpleado (que es el dato local que tenemos)
        return new RondinActivoDto(
            relevo.Id, // Pasamos el RelevoId en lugar del viejo RondinId
            turnoActual.Nombre,
            turnoActual.HoraInicioVentana.ToString(@"hh\:mm"), // Formateo seguro para TimeSpan/TimeOnly
            relevo.Estado,
            relevo.GuardiaSaliente != null
                ? new GuardiaDto(relevo.GuardiaSaliente.SuperAdminUsuarioId, relevo.GuardiaSaliente.NumeroEmpleado)
                : null,
            relevo.GuardiaEntrante != null
                ? new GuardiaDto(relevo.GuardiaEntrante.SuperAdminUsuarioId, relevo.GuardiaEntrante.NumeroEmpleado)
                : null
        );
    }
}