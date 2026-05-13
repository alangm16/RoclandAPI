using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Seguridad;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class SesionService : ISesionService
{
    private readonly SuperAdminDbContext _db;

    public SesionService(SuperAdminDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<SesionActivaDto>> ObtenerSesionesAsync(FiltroSesionesDto filtro)
    {
        var ahora = DateTime.UtcNow;

        var query = _db.RefreshTokens
            .Include(rt => rt.Usuario)
            .Include(rt => rt.Proyecto)
            .Where(rt => !rt.Revocado && rt.FechaExpiracion > ahora)
            .AsQueryable();

        if (filtro.UsuarioId.HasValue)
            query = query.Where(rt => rt.UsuarioId == filtro.UsuarioId.Value);

        if (filtro.ProyectoId.HasValue)
            query = query.Where(rt => rt.ProyectoId == filtro.ProyectoId.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(rt => rt.FechaCreacion)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(rt => new SesionActivaDto(
                rt.Id,
                rt.Usuario.Username,
                rt.Proyecto != null ? rt.Proyecto.Codigo : null,
                rt.Plataforma,
                TokenReducido(rt.Token),
                rt.FechaExpiracion,
                rt.FechaCreacion,
                rt.IpCreacion,
                rt.Revocado
            ))
            .ToListAsync();

        return new PagedResult<SesionActivaDto>(
            items,
            total,
            filtro.Pagina,
            filtro.TamanoPagina
        );
    }

    public async Task RevocarSesionAsync(int refreshTokenId)
    {
        var token = await _db.RefreshTokens.FindAsync(refreshTokenId)
            ?? throw new KeyNotFoundException($"Sesión con Id {refreshTokenId} no encontrada.");

        if (token.Revocado)
            throw new InvalidOperationException("La sesión ya ha sido revocada anteriormente.");

        token.Revocado = true;
        _db.RefreshTokens.Update(token);
        await _db.SaveChangesAsync();
    }

    private static string TokenReducido(string token)
    {
        if (string.IsNullOrEmpty(token)) return string.Empty;
        return token.Length > 10 ? token[^10..] : token;
    }
}