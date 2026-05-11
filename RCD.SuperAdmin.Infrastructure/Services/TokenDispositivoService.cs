using Microsoft.EntityFrameworkCore;
using RCD.Shared.Kernel.Interfaces;
using RCD.SuperAdmin.Application.DTOs.Dispositivos;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class TokenDispositivoService : ITokenDispositivoService
{
    private readonly SuperAdminDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public TokenDispositivoService(SuperAdminDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task RegistrarAsync(RegistrarDispositivoRequest request)
    {
        var usuarioId = _currentUser.GetUserId()
            ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

        var proyectoId = _currentUser.GetProyectoId()
            ?? throw new InvalidOperationException("El token no está asociado a un proyecto.");

        var plataforma = _currentUser.GetPlataforma();

        // Upsert: un solo registro por UsuarioId + ProyectoId + Plataforma
        var existente = await _db.TokensDispositivo
            .FirstOrDefaultAsync(td =>
                td.UsuarioId == usuarioId &&
                td.ProyectoId == proyectoId &&
                td.Plataforma == plataforma);

        if (existente != null)
        {
            existente.FcmToken = request.FcmToken ?? existente.FcmToken;
            existente.DeviceToken = request.DeviceToken ?? existente.DeviceToken;
            existente.DispositivoInfo = request.DispositivoInfo ?? existente.DispositivoInfo;
            existente.Activo = true;
            existente.FechaModificacion = DateTime.UtcNow;
            _db.TokensDispositivo.Update(existente);
        }
        else
        {
            var nuevo = new TokenDispositivo
            {
                UsuarioId = usuarioId,
                ProyectoId = proyectoId,
                Plataforma = plataforma,
                FcmToken = request.FcmToken,
                DeviceToken = request.DeviceToken,
                DispositivoInfo = request.DispositivoInfo,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                FechaModificacion = DateTime.UtcNow
            };
            _db.TokensDispositivo.Add(nuevo);
        }

        await _db.SaveChangesAsync();
    }
}