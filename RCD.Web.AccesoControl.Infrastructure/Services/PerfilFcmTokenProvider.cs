using Microsoft.EntityFrameworkCore;
using RCD.Web.AccesoControl.Infrastructure.Data;
using RCD.Shared.Kernel.Interfaces;

public class PerfilFcmTokenProvider : IPerfilFcmTokenProvider
{
    private readonly AccesoControlWebDbContext _db;

    public PerfilFcmTokenProvider(AccesoControlWebDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<string>> ObtenerTokensActivosAsync(
        IEnumerable<int> superAdminUsuarioIds)
    {
        return await _db.Perfiles
            .Where(p =>
                p.Activo &&
                p.FcmToken != null &&
                superAdminUsuarioIds.Contains(p.SuperAdminUsuarioId))
            .Select(p => p.FcmToken!)
            .ToListAsync();
    }
}