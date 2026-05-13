using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.Dashboard;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly SuperAdminDbContext _db;

    public DashboardService(SuperAdminDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardGlobalDto> GetResumenGlobalAsync()
    {
        var ahora = DateTime.UtcNow;

        // ── Totales de usuarios ────────────────────────────────
        var totalUsuariosActivos = await _db.Usuarios.CountAsync(u => u.Activo);
        var totalUsuariosInactivos = await _db.Usuarios.CountAsync(u => !u.Activo);

        // ── Proyectos activos ──────────────────────────────────
        var totalProyectosActivos = await _db.Proyectos.CountAsync(p => p.Activo);

        // Proyectos por estado (solo activos)
        var estados = await _db.Proyectos
            .Where(p => p.Activo)
            .GroupBy(p => p.Estado)
            .Select(g => new { Estado = g.Key, Count = g.Count() })
            .ToListAsync();

        int produccion = 0, mantenimiento = 0, desarrollo = 0;
        foreach (var e in estados)
        {
            if (e.Estado == "Produccion") produccion = e.Count;
            else if (e.Estado == "Mantenimiento") mantenimiento = e.Count;
            else if (e.Estado == "Desarrollo") desarrollo = e.Count;
        }

        // ── Usuarios bloqueados actualmente ────────────────────
        var usuariosBloqueados = await _db.Usuarios
            .CountAsync(u => u.BloqueadoHasta.HasValue && u.BloqueadoHasta > ahora);

        // ── Alertas críticas no resueltas (total) ──────────────
        var alertasCriticas = await _db.Alertas
            .CountAsync(a => a.Tipo == "critical" && !a.Resuelta);

        // ── Gráfico accesos 7 días ────────────────────────────
        var fechaInicio = ahora.Date.AddDays(-6);
        var logsRecientes = await _db.LogsAcceso
            .Where(l => l.Fecha >= fechaInicio)
            .GroupBy(l => l.Fecha.Date)
            .Select(g => new
            {
                Fecha = g.Key,
                Exitosos = g.Count(l => l.Exitoso),
                Fallidos = g.Count(l => !l.Exitoso)
            })
            .ToListAsync();

        var grafico = Enumerable.Range(0, 7)
            .Select(i => fechaInicio.AddDays(i))
            .Select(f => new GraficoAccesosDto(
                f.ToString("yyyy-MM-dd"),
                logsRecientes.FirstOrDefault(l => l.Fecha == f)?.Exitosos ?? 0,
                logsRecientes.FirstOrDefault(l => l.Fecha == f)?.Fallidos ?? 0
            ));

        // ── Proyectos con más accesos (30 días) ────────────────
        var topProyectos = await _db.LogsAcceso
            .Where(l => l.Fecha >= ahora.AddDays(-30) && l.ProyectoId != null)
            .GroupBy(l => l.ProyectoId)
            .Select(g => new { ProyectoId = g.Key ?? 0, Cantidad = g.Count() })
            .OrderByDescending(x => x.Cantidad)
            .Take(5)
            .Join(_db.Proyectos,
                g => g.ProyectoId,
                p => p.Id,
                (g, p) => new ProyectoActividadDto(g.ProyectoId, p.Nombre, g.Cantidad))
            .ToListAsync();

        // ── Usuarios con más actividad (30 días) ───────────────
        var topUsuarios = await _db.LogsAcceso
            .Where(l => l.Fecha >= ahora.AddDays(-30) && l.UsuarioId != null && l.Exitoso)
            .GroupBy(l => l.UsuarioId)
            .Select(g => new { UsuarioId = g.Key ?? 0, Cantidad = g.Count() })
            .OrderByDescending(x => x.Cantidad)
            .Take(5)
            .Join(_db.Usuarios,
                g => g.UsuarioId,
                u => u.Id,
                (g, u) => new UsuarioActividadDto(g.UsuarioId, u.NombreCompleto, g.Cantidad))
            .ToListAsync();

        // ── Proyectos con alertas críticas no resueltas ────────
        var proyectosConProblemas = await _db.Alertas
            .Where(a => a.Tipo == "critical" && !a.Resuelta && a.ProyectoId != null)
            .GroupBy(a => a.ProyectoId!.Value)
            .Select(g => new
            {
                ProyectoId = g.Key,
                CantidadAlertasCriticas = g.Count()
            })
            .Join(_db.Proyectos,
                g => g.ProyectoId,
                p => p.Id,
                (g, p) => new ProyectoConAlertasDto(
                    g.ProyectoId,
                    p.Codigo,
                    p.Nombre,
                    p.Estado,
                    g.CantidadAlertasCriticas
                ))
            .ToListAsync();

        return new DashboardGlobalDto(
            totalUsuariosActivos,
            totalUsuariosInactivos,
            totalProyectosActivos,
            produccion,
            mantenimiento,
            desarrollo,
            usuariosBloqueados,
            alertasCriticas,
            grafico,
            topProyectos,
            topUsuarios,
            proyectosConProblemas
        );
    }

    public async Task<DashboardProyectoDto> GetDashboardPorProyectoAsync(int proyectoId)
    {
        var proyecto = await _db.Proyectos
            .FirstOrDefaultAsync(p => p.Id == proyectoId && p.Activo)
            ?? throw new KeyNotFoundException($"Proyecto con Id {proyectoId} no encontrado o inactivo.");

        var ahora = DateTime.UtcNow;
        var hoy = ahora.Date;

        // KPIs
        var usuariosAsignados = await _db.ProyectoUsuarioRoles
            .CountAsync(pur => pur.ProyectoId == proyectoId && pur.Activo);

        var rolesDefinidos = await _db.Roles
            .CountAsync(r => r.ProyectoId == proyectoId && r.Activo);

        var vistasConfiguradas = await _db.Vistas
            .CountAsync(v => v.ProyectoId == proyectoId && v.Activo);

        var tokensActivos = await _db.TokensDispositivo
            .CountAsync(td => td.ProyectoId == proyectoId && td.Activo);

        // Últimos accesos
        var ultimosAccesos = await _db.LogsAcceso
            .Where(l => l.ProyectoId == proyectoId && l.Exitoso)
            .OrderByDescending(l => l.Fecha)
            .Take(10)
            .Select(l => new UltimoAccesoDto(l.UsernameUsado, l.Fecha))
            .ToListAsync();

        // Usuarios activos hoy
        var usuariosActivosHoy = await _db.LogsAcceso
            .CountAsync(l => l.ProyectoId == proyectoId
                          && l.Exitoso
                          && l.Fecha >= hoy);

        // Alertas abiertas del proyecto
        var alertasAbiertas = await _db.Alertas
            .CountAsync(a => a.ProyectoId == proyectoId && !a.Resuelta);

        return new DashboardProyectoDto(
            proyecto.Id,
            proyecto.Nombre,
            proyecto.Codigo,
            proyecto.Descripcion ?? string.Empty,
            proyecto.Estado,
            proyecto.Plataforma,
            proyecto.Version,
            proyecto.UrlBase,
            usuariosAsignados,
            rolesDefinidos,
            vistasConfiguradas,
            tokensActivos,
            ultimosAccesos,
            usuariosActivosHoy,
            alertasAbiertas
        );
    }
}