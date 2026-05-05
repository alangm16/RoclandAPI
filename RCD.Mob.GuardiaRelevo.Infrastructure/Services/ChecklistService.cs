using RCD.Mob.GuardiaRelevo.Application.DTOs;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using RCD.Mob.GuardiaRelevo.Domain.Interfaces;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Services;

public class ChecklistService : IChecklistService
{
    private readonly IChecklistRepository _checklist;

    public ChecklistService(IChecklistRepository checklist) => _checklist = checklist;

    public async Task<List<ChecklistCategoriaDto>> ObtenerPuntosAsync(CancellationToken ct = default)
    {
        var puntos = await _checklist.ObtenerPuntosActivosAsync(ct);

        return puntos
            .GroupBy(p => p.Categoria)
            .Select(g => new ChecklistCategoriaDto(
                g.Key,
                g.Select(p => new ChecklistPuntoDto(p.Id, p.Nombre, p.Descripcion ?? "", p.Orden))
                 .ToList()
            ))
            .ToList();
    }

    public async Task<bool> GuardarRespuestasAsync(GuardarRespuestasRequestDto request, CancellationToken ct = default)
    {
        var respuestas = request.Respuestas.Select(r => new ChecklistRespuesta
        {
            RondinId = request.RondinId,
            PuntoId = r.PuntoId,
            Respuesta = r.Respuesta,
            Comentario = r.Comentario,
            FechaRespuesta = DateTime.Now
        }).ToList();

        await _checklist.GuardarRespuestasAsync(respuestas, ct);

        // 👇 Aquí aplicamos el cambio a Observaciones
        if (request.Observaciones is not null)
            await _checklist.ActualizarObservacionesAsync(request.RondinId, request.Observaciones, ct);

        return true;
    }
}