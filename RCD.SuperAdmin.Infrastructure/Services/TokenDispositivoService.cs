using Microsoft.EntityFrameworkCore;
using RCD.Shared.Kernel.Interfaces;
using RCD.SuperAdmin.Application.DTOs;
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

    public async Task RegistrarAsync(RegistrarDispositivoRequest request, string userAgent)
    {
        var finalDispositivoInfo = string.IsNullOrWhiteSpace(request.DispositivoInfo)
            ? userAgent
            : request.DispositivoInfo;

        var usuarioId = _currentUser.GetUserId()
            ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
        var proyectoId = _currentUser.GetProyectoId()
            ?? throw new InvalidOperationException("El token no está asociado a un proyecto.");
        var plataforma = _currentUser.GetPlataforma();

        var existente = await _db.TokensDispositivo
            .FirstOrDefaultAsync(td => td.UsuarioId == usuarioId && td.ProyectoId == proyectoId && td.Plataforma == plataforma);

        if (existente != null)
        {
            existente.FcmToken = request.FcmToken ?? existente.FcmToken;
            existente.DeviceToken = request.DeviceToken ?? existente.DeviceToken;
            existente.DispositivoInfo = finalDispositivoInfo;          // ← usar finalDispositivoInfo
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
                DispositivoInfo = finalDispositivoInfo,  // ← usar finalDispositivoInfo
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                FechaModificacion = DateTime.UtcNow
            };
            _db.TokensDispositivo.Add(nuevo);
        }

        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<DispositivoDto>> ObtenerPorUsuarioAsync(int usuarioId, int pagina = 1, int tamanoPagina = 10)
    {
        const int maxPageSize = 50;
        var pageSize = tamanoPagina > maxPageSize ? maxPageSize : tamanoPagina;

        var query = _db.TokensDispositivo
            .Include(td => td.Proyecto)
            .Where(td => td.UsuarioId == usuarioId);
        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(td => td.FechaCreacion)
            .Skip((pagina - 1) * pageSize)
            .Take(pageSize)
            .Select(td => new DispositivoDto(
                td.Id,
                td.Plataforma,
                td.DeviceToken,
                td.FcmToken,
                td.DispositivoInfo,
                td.Proyecto.Codigo,
                td.Activo,
                td.FechaCreacion
            ))
            .ToListAsync();

        return new PagedResult<DispositivoDto>(items, total, pagina, pageSize);
    }

    public async Task RevocarDispositivoAsync(int dispositivoId)
    {
        var dispositivo = await _db.TokensDispositivo.FindAsync(dispositivoId)
            ?? throw new KeyNotFoundException($"Dispositivo con Id {dispositivoId} no encontrado.");

        if (!dispositivo.Activo)
            throw new InvalidOperationException("El dispositivo ya está revocado.");

        dispositivo.Activo = false;
        dispositivo.FechaModificacion = DateTime.UtcNow;
        _db.TokensDispositivo.Update(dispositivo);
        await _db.SaveChangesAsync();
    }
}