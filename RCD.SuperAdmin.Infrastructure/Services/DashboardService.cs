using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.Dashboard;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Infrastructure.Data;
using System.Globalization;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly SuperAdminDbContext _db;

    public DashboardService(SuperAdminDbContext db)
    {
        _db = db;
    }

    public async Task<SuperAdminDashboardKpisDto> GetKpisAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        return new SuperAdminDashboardKpisDto
        {
            UsuariosActivos = await _db.Usuarios.CountAsync(u => u.Activo),
            ModulosTotales = await _db.Proyectos.CountAsync(p => p.Activo),
            // Corregido: Usamos l.Fecha
            AccesosDiarios = await _db.LogsAcceso.CountAsync(l => l.Fecha >= hoy && l.Exitoso),
            AlertasAbiertas = await _db.Alertas.CountAsync(a => !a.Resuelta),
            NuevosUsuariosMes = await _db.Usuarios.CountAsync(u => u.FechaCreacion >= inicioMes)
        };
    }

    public async Task<IEnumerable<AccesosDiaSemanaDto>> GetAccesosSemanaAsync()
    {
        var fechaInicio = DateTime.UtcNow.Date.AddDays(-6);

        // Corregido: Usamos l.Fecha.Date
        var logs = await _db.LogsAcceso
            .Where(l => l.Fecha >= fechaInicio)
            .GroupBy(l => l.Fecha.Date)
            .Select(g => new {
                Fecha = g.Key,
                Exitosos = g.Count(x => x.Exitoso),
                Fallidos = g.Count(x => !x.Exitoso)
            })
            .ToListAsync();

        var resultado = new List<AccesosDiaSemanaDto>();
        var cultura = new CultureInfo("es-MX");

        for (int i = 0; i < 7; i++)
        {
            var dia = fechaInicio.AddDays(i);
            var logDia = logs.FirstOrDefault(l => l.Fecha == dia);

            resultado.Add(new AccesosDiaSemanaDto
            {
                Dia = cultura.DateTimeFormat.GetAbbreviatedDayName(dia.DayOfWeek),
                Exitosos = logDia?.Exitosos ?? 0,
                Fallidos = logDia?.Fallidos ?? 0
            });
        }
        return resultado;
    }

    public async Task<IEnumerable<ModuloUsageDto>> GetUsoModulosAsync()
    {
        return await _db.Proyectos
            .Where(p => p.Activo)
            .Select(p => new ModuloUsageDto
            {
                ProyectoId = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Plataforma = p.Plataforma,
                IconoCss = p.IconoCss,
                Version = p.Version,
                Estado = p.Estado,
                UsuariosActivos = _db.PermisosUsuario.Count(pu => pu.ProyectoId == p.Id)
            }).ToListAsync();
    }

    // ── NUEVOS MÉTODOS CORREGIDOS ────────────────────────────────────────

    public async Task<IEnumerable<AlertaDto>> GetAlertasActivasAsync()
    {
        return await _db.Alertas
            .Where(a => !a.Resuelta)
            .OrderByDescending(a => a.Fecha)
            .Select(a => new AlertaDto
            {
                Id = a.Id,
                Tipo = a.Tipo,
                Titulo = a.Titulo,
                Mensaje = a.Mensaje,
                Fecha = a.Fecha,
                Resuelta = a.Resuelta,
                AccionUrl = a.AccionUrl
            }).ToListAsync();
    }

    public async Task<IEnumerable<AccesoLogDto>> GetLogsRecientesAsync(int limit)
    {
        return await _db.LogsAcceso
            .Include(l => l.Usuario) // Necesario para traer NombreCompleto
            .OrderByDescending(l => l.Fecha)
            .Take(limit)
            .Select(l => new AccesoLogDto
            {
                UsuarioId = l.UsuarioId,
                // Si el usuario no existe (ej. un intento de hackeo), mostramos "Usuario no registrado"
                NombreCompleto = l.Usuario != null ? l.Usuario.NombreCompleto : "Desconocido",
                Username = l.UsernameUsado,
                Exitoso = l.Exitoso,
                IpAddress = l.IpAddress ?? "0.0.0.0",
                Plataforma = l.Plataforma ?? "Global",
                Detalle = l.Detalle ?? "Intento de acceso",
                Fecha = l.Fecha
            }).ToListAsync();
    }
}