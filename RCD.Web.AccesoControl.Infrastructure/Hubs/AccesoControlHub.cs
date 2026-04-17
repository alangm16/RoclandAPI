using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RCD.Web.AccesoControl.Infrastructure.Hubs;

[Authorize(AuthenticationSchemes = "AdminCookie,Bearer", Roles = "Guardia,Admin,Supervisor")]
public class AccesoControlHub : Hub
{
    public async Task UnirseAGuardias()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Guardias");
    }

    public async Task ConfirmarRecepcion(int solicitudId)
    {
        await Clients.Group("Guardias")
            .SendAsync("SolicitudConfirmada", solicitudId);
    }

    public async Task NotificarSalida(int registroId)
    {
        await Clients.Group("Guardias")
            .SendAsync("SalidaRegistrada", registroId);
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Guardias");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Guardias");
        await base.OnDisconnectedAsync(exception);
    }
}