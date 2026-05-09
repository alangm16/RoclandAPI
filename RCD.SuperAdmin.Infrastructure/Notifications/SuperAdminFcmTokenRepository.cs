using Microsoft.EntityFrameworkCore;
using RCD.Shared.Infrastructure.Notifications;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Notifications;

public class SuperAdminFcmTokenRepository(SuperAdminDbContext db) : IFcmTokenRepository
{
    public async Task InvalidarTokenAsync(string fcmToken)
    {
        var token = await db.TokensDispositivo
            .FirstOrDefaultAsync(t => t.FcmToken == fcmToken);

        if (token is null) return;

        token.FcmToken = null;
        token.Activo = false;
        token.FechaModificacion = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}