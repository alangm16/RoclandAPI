using MediatR;
using Microsoft.Extensions.Logging;
using RCD.Shared.Kernel.Events;

namespace RCD.Web.AccesoControl.Application.Handlers;

// INotificationHandler le dice a MediatR: "Avísame cuando ocurra este evento"
public class VisitanteRegistradoHandler : INotificationHandler<VisitanteRegistradoEvent>
{
    private readonly ILogger<VisitanteRegistradoHandler> _logger;

    public VisitanteRegistradoHandler(ILogger<VisitanteRegistradoHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(VisitanteRegistradoEvent notification, CancellationToken cancellationToken)
    {
        // Aquí podrías enviar un email, actualizar un KPI en caché, etc.
        _logger.LogWarning("⭐⭐⭐ [BUS DE EVENTOS] Escuché que el visitante {Nombre} acaba de registrarse para: {Motivo} a las {Fecha}",
            notification.NombrePersona,
            notification.Motivo,
            notification.FechaHora);

        return Task.CompletedTask;
    }
}