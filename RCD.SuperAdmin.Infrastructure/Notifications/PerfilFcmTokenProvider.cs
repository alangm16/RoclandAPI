// RCD.SuperAdmin.Infrastructure/Notifications/PerfilFcmTokenProvider.cs
using Microsoft.EntityFrameworkCore;
using RCD.Shared.Infrastructure.Notifications;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Notifications;

public class PerfilFcmTokenProvider(SuperAdminDbContext db) : IPerfilFcmTokenProvider
{
    public async Task<IEnumerable<string>> ObtenerTokensActivosAsync(IEnumerable<int> superAdminUsuarioIds)
    {
        // Buscamos en la tabla global de SuperAdmin los tokens FCM
        // de los usuarios que AccesoControl nos está solicitando
        return await db.TokensDispositivo
            .Where(t => superAdminUsuarioIds.Contains(t.UsuarioId)
                     && t.Activo
                     && !string.IsNullOrEmpty(t.FcmToken))
            .Select(t => t.FcmToken!)
            .ToListAsync();
    }
}