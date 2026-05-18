using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RCD.Shared.Kernel.Interfaces;
using RCD.Web.AccesoControl.Domain.Models.Entities.Base;

namespace RCD.Web.AccesoControl.Infrastructure.Settings;

public class AccesoControlAuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public AccesoControlAuditInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AplicarAuditoria(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // Es importante atrapar también los métodos síncronos
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AplicarAuditoria(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void AplicarAuditoria(DbContext? context)
    {
        if (context == null) return;

        var userId = _currentUser.GetUserId();

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.FechaCreacion = DateTime.UtcNow;
                    entry.Entity.FechaModificacion = null;
                    entry.Entity.CreadoPor = userId;
                    entry.Entity.Activo = true;
                    break;

                case EntityState.Modified:
                    entry.Entity.FechaModificacion = DateTime.UtcNow;
                    entry.Entity.ModificadoPor = userId;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.Activo = false;
                    entry.Entity.FechaModificacion = DateTime.UtcNow;
                    entry.Entity.ModificadoPor = userId;
                    break;
            }
        }
    }
}