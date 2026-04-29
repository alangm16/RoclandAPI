using MediatR;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using RCD.Mob.GuardiaRelevo.Domain.Interfaces;

namespace RCD.Mob.GuardiaRelevo.Application.Rondines;

public class ValidarQRHandler : IRequestHandler<ValidarQRCommand, ValidarQRResultDto>
{
    private readonly IRondinRepository _rondines;
    private readonly IUsuarioRepository _usuarios;

    public ValidarQRHandler(IRondinRepository rondines, IUsuarioRepository usuarios)
    {
        _rondines = rondines;
        _usuarios = usuarios;
    }

    public async Task<ValidarQRResultDto> Handle(ValidarQRCommand request, CancellationToken ct)
    {
        // 1. Obtener el rondín
        var rondin = await _rondines.ObtenerPorIdAsync(request.RondinId, ct);
        if (rondin is null)
            return new(false, "Rondín no encontrado.");

        if (rondin.Estado == "Completado" || rondin.Estado == "Cancelado")
            return new(false, "El rondín ya fue cerrado.");

        // 2. Validar que el QR corresponde al guardia correcto
        var usuario = await _usuarios.ObtenerPorQRAsync(request.QRCode, ct);
        if (usuario is null)
            return new(false, "Código QR no reconocido.");

        var idEsperado = request.TipoGuardia == "Saliente"
            ? rondin.GuardiaSalienteId
            : rondin.GuardiaEntranteId;

        if (usuario.Id != idEsperado)
            return new(false, $"El QR no corresponde al guardia {request.TipoGuardia.ToLower()}.");

        // 3. Verificar que no haya escaneado ya
        var yaEscaneo = rondin.Eventos.Any(e =>
            e.TipoEvento == "QR" &&
            e.TipoGuardia == request.TipoGuardia &&
            e.UsuarioId == usuario.Id &&
            e.Exitoso);

        if (yaEscaneo)
            return new(false, $"El guardia {request.TipoGuardia.ToLower()} ya escaneó su QR.");

        // 4. Registrar evento QR
        await _rondines.RegistrarEventoAsync(new RondinEvento
        {
            RondinId = request.RondinId,
            UsuarioId = usuario.Id,
            TipoEvento = "QR",
            TipoGuardia = request.TipoGuardia,
            Exitoso = true,
            FechaEvento = DateTime.Now
        }, ct);

        // 5. Si ambos QR ya escanearon → pasar a EnCurso
        var qrSaliente = rondin.Eventos.Any(e => e.TipoEvento == "QR" && e.TipoGuardia == "Saliente" && e.Exitoso);
        var esSaliente = request.TipoGuardia == "Saliente";
        var qrEntrante = rondin.Eventos.Any(e => e.TipoEvento == "QR" && e.TipoGuardia == "Entrante" && e.Exitoso);
        var esEntrante = request.TipoGuardia == "Entrante";

        // El evento que acabamos de guardar ya cuenta
        var ambosEscanearon = (qrSaliente || esSaliente) && (qrEntrante || esEntrante);

        if (ambosEscanearon)
        {
            await _rondines.ActualizarEstadoAsync(request.RondinId, "EnCurso", ct);
        }

        return new(true, $"QR del guardia {request.TipoGuardia.ToLower()} validado.", usuario.Id);
    }
}