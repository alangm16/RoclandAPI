using MediatR;

namespace RCD.Shared.Kernel.Events;

// INotification le dice a MediatR que esto es un evento (se envía a todos los que escuchen)
public record VisitanteRegistradoEvent(
    int RegistroId,
    string NombrePersona,
    string Motivo,
    DateTime FechaHora
) : INotification;