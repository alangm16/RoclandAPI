using MediatR;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
// using RCD.Mob.GuardiaRelevo.Domain.Interfaces; // Asegúrate de tener IRelevoRepository

namespace RCD.Mob.GuardiaRelevo.Application.Rondines;

public class ValidarQRHandler : IRequestHandler<ValidarQRCommand, ValidarQRResultDto>
{
    // Cambiamos a IRelevoRepository (necesitarás crearlo si aún lo llamas IRondinRepository)
    private readonly IRelevoRepository _relevos;
    private readonly IAuthService _auth;

    public ValidarQRHandler(IRelevoRepository relevos, IAuthService auth)
    {
        _relevos = relevos;
        _auth = auth;
    }

    public async Task<ValidarQRResultDto> Handle(ValidarQRCommand request, CancellationToken ct)
    {
        // 1. Obtener el Relevo (que agrupa a Saliente y Entrante)
        var relevo = await _relevos.ObtenerPorIdAsync(request.RelevoId, ct);
        if (relevo is null)
            return new(false, "Relevo no encontrado.");

        if (relevo.Estado == "Completado" || relevo.Estado == "Incompleto")
            return new(false, "El relevo ya fue cerrado.");

        // 2. Validar el QR consultando al dominio de SuperAdmin
        var usuarioQRId = await _auth.ObtenerSuperAdminIdPorQRAsync(request.QRCode);
        if (usuarioQRId == null)
            return new(false, "Código QR no reconocido o inactivo.");

        // 3. Validar que el QR corresponde al guardia esperado en el relevo
        var idEsperado = request.TipoGuardia == "Saliente"
            ? relevo.GuardiaSalienteId
            : relevo.GuardiaEntranteId;

        // Validamos contra la nueva llave foránea
        if (usuarioQRId != idEsperado)
            return new(false, $"El QR no corresponde al guardia {request.TipoGuardia.ToLower()} asignado a este relevo.");

        // 4. Cambiar estado del relevo si apenas va a comenzar
        // Ya no insertamos en TBL_ROCLAND_RELEVO_RONDIN_EVENTOS porque fue eliminada.
        if (relevo.Estado == "Pendiente")
        {
            await _relevos.ActualizarEstadoAsync(relevo.Id, "EnCurso", ct);
        }

        return new(true, $"QR del guardia {request.TipoGuardia.ToLower()} validado correctamente.", usuarioQRId.Value);
    }
}