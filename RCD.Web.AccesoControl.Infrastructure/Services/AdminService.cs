using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RCD.Web.AccesoControl.Infrastructure.Data;
using RCD.Web.AccesoControl.Application.DTOs;
using RCD.Web.AccesoControl.Domain.Models.Entities;
using RCD.Web.AccesoControl.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace RCD.Web.AccesoControl.Web.Services;

public class AdminService : IAdminService
{
    private readonly AccesoControlWebDbContext _db;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan _offsetMexico = TimeSpan.FromHours(-6);

    private const string CacheKeyAreas = "Catalogos_Areas";
    private const string CacheKeyTiposId = "Catalogos_TiposId";
    private const string CacheKeyMotivos = "Catalogos_Motivos";

    public AdminService(AccesoControlWebDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    // ── KPIs ───────────────────────────────────────────────────────────
    public async Task<DashboardKpiDto> ObtenerKpisAsync()
    {
        var ahoraLocal = DateTime.UtcNow.Add(_offsetMexico);
        var hoyLocal = ahoraLocal.Date;
        var inicioDiaUtc = hoyLocal.AddHours(6);
        var finDiaUtc = inicioDiaUtc.AddDays(1);

        var dentroAhora = await _db.RegistrosVisitantes
            .CountAsync(r => r.EstadoAcceso == "Aprobado" && r.FechaSalida == null)
            + await _db.RegistrosProveedores
            .CountAsync(r => r.EstadoAcceso == "Aprobado" && r.FechaSalida == null);

        var visitantesHoy = await _db.RegistrosVisitantes
            .CountAsync(r => r.FechaEntrada >= inicioDiaUtc && r.FechaEntrada < finDiaUtc);

        var proveedoresHoy = await _db.RegistrosProveedores
            .CountAsync(r => r.FechaEntrada >= inicioDiaUtc && r.FechaEntrada < finDiaUtc);

        var pendientes = await _db.SolicitudesPendientes
            .CountAsync(s => s.Estado == "Pendiente");

        var tiemposVis = await _db.RegistrosVisitantes
            .Where(r => r.FechaEntrada >= inicioDiaUtc && r.FechaEntrada < finDiaUtc && r.FechaSalida != null)
            .Select(r => new { r.FechaEntrada, r.FechaSalida })
            .ToListAsync();

        var tiemposProv = await _db.RegistrosProveedores
            .Where(r => r.FechaEntrada >= inicioDiaUtc && r.FechaEntrada < finDiaUtc && r.FechaSalida != null)
            .Select(r => new { r.FechaEntrada, r.FechaSalida })
            .ToListAsync();

        var todosLosMinutos = tiemposVis.Concat(tiemposProv)
            .Select(x => (x.FechaSalida!.Value - x.FechaEntrada).TotalMinutes)
            .ToList();

        var promedio = todosLosMinutos.Count > 0 ? todosLosMinutos.Average() : 0;

        return new DashboardKpiDto(
            dentroAhora,
            visitantesHoy + proveedoresHoy,
            visitantesHoy,
            proveedoresHoy,
            Math.Round(promedio, 1),
            pendientes);
    }

    public async Task<IEnumerable<FlujoPorHoraDto>> ObtenerFlujoPorHoraHoyAsync()
    {
        var ahoraLocal = DateTime.UtcNow.Add(_offsetMexico);
        var hoyLocal = ahoraLocal.Date;
        var inicioDiaUtc = hoyLocal.AddHours(6);
        var finDiaUtc = inicioDiaUtc.AddDays(1);

        var visFechas = await _db.RegistrosVisitantes
            .Where(r => r.FechaEntrada >= inicioDiaUtc && r.FechaEntrada < finDiaUtc)
            .Select(r => r.FechaEntrada)
            .ToListAsync();

        var provFechas = await _db.RegistrosProveedores
            .Where(r => r.FechaEntrada >= inicioDiaUtc && r.FechaEntrada < finDiaUtc)
            .Select(r => r.FechaEntrada)
            .ToListAsync();

        var visAgrupados = visFechas
            .GroupBy(f => f.Add(_offsetMexico).Hour)
            .ToDictionary(g => g.Key, g => g.Count());

        var provAgrupados = provFechas
            .GroupBy(f => f.Add(_offsetMexico).Hour)
            .ToDictionary(g => g.Key, g => g.Count());

        return Enumerable.Range(6, 16)
            .Select(h => new FlujoPorHoraDto(h,
                visAgrupados.GetValueOrDefault(h, 0) +
                provAgrupados.GetValueOrDefault(h, 0)));
    }

    public async Task<IEnumerable<FlujoDiarioDto>> ObtenerFlujoDiarioMesAsync(int anio, int mes)
    {
        var inicioMesLocal = new DateTime(anio, mes, 1);
        var finMesLocal = inicioMesLocal.AddMonths(1);
        var inicioMesUtc = inicioMesLocal.AddHours(6);
        var finMesUtc = finMesLocal.AddHours(6);

        var visFechas = await _db.RegistrosVisitantes
            .Where(r => r.FechaEntrada >= inicioMesUtc && r.FechaEntrada < finMesUtc)
            .Select(r => r.FechaEntrada)
            .ToListAsync();

        var provFechas = await _db.RegistrosProveedores
            .Where(r => r.FechaEntrada >= inicioMesUtc && r.FechaEntrada < finMesUtc)
            .Select(r => r.FechaEntrada)
            .ToListAsync();

        var visAgrupados = visFechas
            .GroupBy(f => f.Add(_offsetMexico).Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var provAgrupados = provFechas
            .GroupBy(f => f.Add(_offsetMexico).Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var diasEnMes = DateTime.DaysInMonth(anio, mes);
        return Enumerable.Range(1, diasEnMes).Select(d =>
        {
            var fechaLocal = new DateTime(anio, mes, d);
            return new FlujoDiarioDto(
                fechaLocal.ToString("dd/MM"),
                visAgrupados.GetValueOrDefault(fechaLocal, 0),
                provAgrupados.GetValueOrDefault(fechaLocal, 0));
        });
    }

    public async Task<IEnumerable<AreaVisitadaDto>> ObtenerAreasMasVisitadasAsync(int dias = 30)
    {
        var ahoraLocal = DateTime.UtcNow.Add(_offsetMexico);
        var desdeUtc = ahoraLocal.Date.AddDays(-dias).AddHours(6);

        var topAreas = await _db.RegistrosVisitantes
            .Where(r => r.FechaEntrada >= desdeUtc)
            .GroupBy(r => r.AreaId)
            .Select(g => new { AreaId = g.Key, Total = g.Count() })
            .OrderByDescending(x => x.Total)
            .Take(8)
            .ToListAsync();

        var areaIds = topAreas.Select(x => x.AreaId).ToList();
        var areasDict = await _db.Areas
            .Where(a => areaIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Nombre);

        return topAreas.Select(x => new AreaVisitadaDto(
            areasDict.GetValueOrDefault(x.AreaId, "Desconocido"),
            x.Total
        )).ToList();
    }

    // ── Historial ──────────────────────────────────────────────────────
    public async Task<(IEnumerable<HistorialAccesoDto> Items, int Total)> ObtenerHistorialAsync(
    string? busqueda, string? tipo, DateTime? desde, DateTime? hasta,
    int pagina, int porPagina)
    {
        hasta = hasta?.Date.AddDays(1);

        // ── FIX: Usamos PerfilEntrada en los Includes
        IQueryable<RegistroVisitante> BaseVisitantes() =>
            _db.RegistrosVisitantes
                .Include(r => r.Persona).ThenInclude(p => p.TipoIdentificacion)
                .Include(r => r.Area)
                .Include(r => r.Motivo)
                .Include(r => r.PerfilEntrada)
                .Include(r => r.Gafete)
                .Where(r => (tipo == null || tipo == "Visitante") &&
                            (desde == null || r.FechaEntrada >= desde) &&
                            (hasta == null || r.FechaEntrada < hasta) &&
                            (busqueda == null ||
                             r.Persona.Nombre.Contains(busqueda) ||
                             r.Persona.NumeroIdentificacion.Contains(busqueda)));

        IQueryable<RegistroProveedor> BaseProveedores() =>
            _db.RegistrosProveedores
                .Include(r => r.Persona).ThenInclude(p => p.TipoIdentificacion)
                .Include(r => r.Motivo)
                .Include(r => r.PerfilEntrada)
                .Include(r => r.Gafete)
                .Where(r => (tipo == null || tipo == "Proveedor") &&
                            (desde == null || r.FechaEntrada >= desde) &&
                            (hasta == null || r.FechaEntrada < hasta) &&
                            (busqueda == null ||
                             r.Persona.Nombre.Contains(busqueda) ||
                             r.Persona.NumeroIdentificacion.Contains(busqueda) ||
                             (r.Persona.Empresa != null && r.Persona.Empresa.Contains(busqueda))));

        List<HistorialAccesoDto> listaVisitantes = new();
        List<HistorialAccesoDto> listaProveedores = new();

        if (tipo == null || tipo == "Visitante")
        {
            listaVisitantes = await BaseVisitantes()
                .Select(r => new HistorialAccesoDto(
                    r.Id,
                    "Visitante",
                    r.Persona.Nombre,
                    null,
                    r.Persona.NumeroIdentificacion,
                    r.Area.Nombre,
                    r.Motivo.Nombre,
                    r.FechaEntrada,
                    r.FechaSalida,
                    r.MinutosEstancia,
                    r.EstadoAcceso,
                    r.Gafete != null ? r.Gafete.Codigo : null,
                    r.PerfilEntrada != null ? r.PerfilEntrada.NombreCompleto : "Desconocido")) // ── FIX: Usamos NombreCompleto
                .ToListAsync();
        }

        if (tipo == null || tipo == "Proveedor")
        {
            listaProveedores = await BaseProveedores()
                .Select(r => new HistorialAccesoDto(
                    r.Id,
                    "Proveedor",
                    r.Persona.Nombre,
                    r.Persona.Empresa,
                    r.Persona.NumeroIdentificacion,
                    null,
                    r.Motivo.Nombre,
                    r.FechaEntrada,
                    r.FechaSalida,
                    r.MinutosEstancia,
                    r.EstadoAcceso,
                    r.Gafete != null ? r.Gafete.Codigo : null,
                    r.PerfilEntrada != null ? r.PerfilEntrada.NombreCompleto : "Desconocido")) // ── FIX: Usamos NombreCompleto
                .ToListAsync();
        }

        var union = listaVisitantes.Concat(listaProveedores)
                                   .OrderByDescending(r => r.FechaEntrada)
                                   .ToList();

        var total = union.Count;
        var itemsUtc = union.Skip((pagina - 1) * porPagina)
                         .Take(porPagina)
                         .ToList();

        var items = itemsUtc.Select(r => new HistorialAccesoDto(
            r.Id, r.Tipo, r.Nombre, r.Empresa, r.NumeroIdentificacion, r.Area, r.Motivo,
            r.FechaEntrada.Add(_offsetMexico),
            r.FechaSalida.HasValue ? r.FechaSalida.Value.Add(_offsetMexico) : null,
            r.MinutosEstancia, r.EstadoAcceso, r.CodigoGafete, r.Guardia // Nota: la variable r.Guardia en el record tiene el NombreCompleto
        )).ToList();

        return (items, total);
    }

    // ── Personas ───────────────────────────────────────────────────────
    public async Task<(IEnumerable<PersonaPerfilDto> Items, int Total)> ObtenerPersonasPaginadasAsync(string? busqueda, int pagina, int porPagina)
    {
        var query = _db.Personas
            .Include(p => p.TipoIdentificacion)
            .AsNoTracking()
            .Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            busqueda = busqueda.ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(busqueda) ||
                p.NumeroIdentificacion.ToLower().Contains(busqueda) ||
                (p.Empresa != null && p.Empresa.ToLower().Contains(busqueda))
            );
        }

        var total = await query.CountAsync();

        // 1. Traemos los datos de la BD en formato anónimo (UTC)
        var itemsDb = await query
            .OrderByDescending(p => p.TotalVisitas)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(p => new {
                p.Id,
                p.Nombre,
                TipoId = p.TipoIdentificacion.Nombre,
                p.NumeroIdentificacion,
                p.Empresa,
                p.Telefono,
                p.Email,
                p.TotalVisitas,
                p.FechaRegistro,
                p.FechaUltimaVisita
            })
            .ToListAsync();

        // 2. Mapeamos al DTO aplicando la conversión de horas
        var items = itemsDb.Select(p => new PersonaPerfilDto(
            p.Id, p.Nombre, p.TipoId, p.NumeroIdentificacion, p.Empresa, p.Telefono, p.Email,
            p.TotalVisitas,
            p.FechaRegistro.Add(_offsetMexico),
            p.FechaUltimaVisita.HasValue ? p.FechaUltimaVisita.Value.Add(_offsetMexico) : null
        )).ToList();

        return (items, total);
    }

    public async Task<PersonaPerfilDto?> ObtenerPerfilPersonaAsync(int id)
    {
        var p = await _db.Personas
            .Include(p => p.TipoIdentificacion)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (p is null) return null;

        // Aplicamos el _offsetMexico a las fechas
        return new PersonaPerfilDto(
            p.Id,
            p.Nombre,
            p.TipoIdentificacion.Nombre,
            p.NumeroIdentificacion,
            p.Empresa,
            p.Telefono,
            p.Email,
            p.TotalVisitas,
            p.FechaRegistro.Add(_offsetMexico),
            p.FechaUltimaVisita.HasValue ? p.FechaUltimaVisita.Value.Add(_offsetMexico) : null);
    }

    public async Task<IEnumerable<HistorialAccesoDto>> ObtenerHistorialPersonaAsync(int personaId)
    {
        // 1. Consultar Visitantes 
        var visitasDb = await _db.RegistrosVisitantes
            .Where(r => r.PersonaId == personaId)
            .Select(r => new {
                r.Id,
                r.Persona.Nombre,
                r.Persona.NumeroIdentificacion,
                Area = r.Area.Nombre,
                Motivo = r.Motivo.Nombre,
                r.FechaEntrada,
                r.FechaSalida,
                r.MinutosEstancia,
                r.EstadoAcceso,
                CodigoGafete = r.Gafete != null ? r.Gafete.Codigo : null,
                Guardia = r.PerfilEntrada != null ? r.PerfilEntrada.NombreCompleto : "Desconocido"
            })
            .ToListAsync();

        // Convertimos fechas de visitantes (Aplicando el Offset)
        var visitas = visitasDb.Select(r => new HistorialAccesoDto(
            r.Id, "Visitante", r.Nombre, null, r.NumeroIdentificacion,
            r.Area, r.Motivo,
            r.FechaEntrada.Add(_offsetMexico),
            r.FechaSalida.HasValue ? r.FechaSalida.Value.Add(_offsetMexico) : null,
            r.MinutosEstancia, r.EstadoAcceso, r.CodigoGafete, r.Guardia
        ));

        // 2. Consultar Proveedores
        var proveedoresDb = await _db.RegistrosProveedores
            .Where(r => r.PersonaId == personaId)
            .Select(r => new {
                r.Id,
                r.Persona.Nombre,
                r.Persona.Empresa,
                r.Persona.NumeroIdentificacion,
                Motivo = r.Motivo.Nombre,
                r.FechaEntrada,
                r.FechaSalida,
                r.MinutosEstancia,
                r.EstadoAcceso,
                CodigoGafete = r.Gafete != null ? r.Gafete.Codigo : null,
                Guardia = r.PerfilEntrada != null ? r.PerfilEntrada.NombreCompleto : "Desconocido"
            })
            .ToListAsync();

        // Convertimos fechas de proveedores (Aplicando el Offset)
        var proveedores = proveedoresDb.Select(r => new HistorialAccesoDto(
            r.Id, "Proveedor", r.Nombre, r.Empresa, r.NumeroIdentificacion,
            null, r.Motivo,
            r.FechaEntrada.Add(_offsetMexico),
            r.FechaSalida.HasValue ? r.FechaSalida.Value.Add(_offsetMexico) : null,
            r.MinutosEstancia, r.EstadoAcceso, r.CodigoGafete, r.Guardia
        ));

        // 3. Unir y ordenar
        return visitas.Concat(proveedores).OrderByDescending(r => r.FechaEntrada);
    }

    // ── Guardias (Refactorizado a Perfiles) ────────────────────────────
    public async Task<(IEnumerable<GuardiaListDto> Items, int Total)> ObtenerGuardiasAsync(string? busqueda, int pagina, int porPagina)
    {
        // ── FIX: Consultamos la tabla Perfiles
        var query = _db.Perfiles.AsQueryable().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            busqueda = busqueda.ToLower();
            query = query.Where(g =>
                g.NombreCompleto.ToLower().Contains(busqueda) ||
                (g.NumeroEmpleado != null && g.NumeroEmpleado.ToLower().Contains(busqueda)));
        }

        var total = await query.CountAsync();

        // ── FIX: Adaptamos el mapeo al DTO original (reutilizamos la propiedad Usuario para mostrar el TipoPerfil o No.Empleado)
        var items = await query
            .OrderByDescending(g => g.Activo)
            .ThenBy(g => g.NombreCompleto)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(g => new GuardiaListDto(
                g.Id,
                g.NombreCompleto,
                g.NumeroEmpleado ?? "N/A",
                g.Turno ?? "Sin turno",   // Rol ← antes g.TipoPerfil
                g.Activo,
                g.FechaCreacion
            ))
            .ToListAsync();

        return (items, total);
    }

    public async Task<bool> ActualizarGuardiaAsync(int id, GuardiaUpdateDto dto)
    {
        var perfil = await _db.Perfiles.FindAsync(id);
        if (perfil is null) return false;

        perfil.NumeroEmpleado = dto.NumeroEmpleado ?? perfil.NumeroEmpleado;
        perfil.Turno = dto.Turno ?? perfil.Turno;

        return await _db.SaveChangesAsync() > 0;
    }

    // ── Catálogos ──────────────────────────────────────────────────────
    public async Task<IEnumerable<AreaDto>> GetAreasAsync()
    {
        return await _db.Areas
            .OrderBy(a => a.Nombre)
            .Select(a => new AreaDto(a.Id, a.Nombre, a.Activo))
            .ToListAsync();
    }

    public async Task<bool> CrearAreaAsync(CatalogoCreateDto dto)
    {
        _db.Areas.Add(new Area { Nombre = dto.Nombre });
        var ok = await _db.SaveChangesAsync() > 0;

        if (ok) _cache.Remove(CacheKeyAreas);
        return ok;
    }

    public async Task<bool> ToggleAreaAsync(int id)
    {
        var area = await _db.Areas.FindAsync(id);
        if (area is null) return false;
        area.Activo = !area.Activo;
        var ok = await _db.SaveChangesAsync() > 0;

        if (ok) _cache.Remove(CacheKeyAreas);
        return ok;
    }

    public async Task<IEnumerable<MotivoDto>> GetMotivosAsync()
    {
        return await _db.MotivosVisita
            .OrderBy(m => m.Nombre)
            .Select(m => new MotivoDto(m.Id, m.Nombre, m.Activo))
            .ToListAsync();
    }

    public async Task<bool> CrearMotivoAsync(CatalogoCreateDto dto)
    {
        _db.MotivosVisita.Add(new MotivoVisita { Nombre = dto.Nombre });
        var ok = await _db.SaveChangesAsync() > 0;

        if (ok) _cache.Remove(CacheKeyMotivos);
        return ok;
    }

    public async Task<bool> ToggleMotivoAsync(int id)
    {
        var motivo = await _db.MotivosVisita.FindAsync(id);
        if (motivo is null) return false;
        motivo.Activo = !motivo.Activo;
        var ok = await _db.SaveChangesAsync() > 0;

        if (ok) _cache.Remove(CacheKeyMotivos);
        return ok;
    }

    public async Task<IEnumerable<TipoIdDto>> GetTiposIdAsync()
    {
        return await _db.TiposIdentificacion
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoIdDto(t.Id, t.Nombre, t.Activo))
            .ToListAsync();
    }

    public async Task<bool> CrearTipoIdAsync(CatalogoCreateDto dto)
    {
        _db.TiposIdentificacion.Add(new TipoIdentificacion { Nombre = dto.Nombre });
        var ok = await _db.SaveChangesAsync() > 0;

        if (ok) _cache.Remove(CacheKeyTiposId);
        return ok;
    }

    public async Task<bool> ToggleTipoIdAsync(int id)
    {
        var tipo = await _db.TiposIdentificacion.FindAsync(id);
        if (tipo is null) return false;
        tipo.Activo = !tipo.Activo;
        var ok = await _db.SaveChangesAsync() > 0;

        if (ok) _cache.Remove(CacheKeyTiposId);
        return ok;
    }

    // ── Exportar Excel ─────────────────────────────────────────────────
    public async Task<byte[]> ExportarExcelHoyAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var (items, _) = await ObtenerHistorialAsync(null, null, hoy, hoy, 1, int.MaxValue);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Accesos del día");

        var headers = new[]
        {
            "Tipo", "Nombre", "Empresa", "No. ID",
            "Área / Empresa", "Motivo", "Entrada", "Salida",
            "Minutos", "Estado", "Gafete", "Guardia"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var item in items)
        {
            ws.Cell(row, 1).Value = item.Tipo;
            ws.Cell(row, 2).Value = item.Nombre;
            ws.Cell(row, 3).Value = item.Empresa ?? "";
            ws.Cell(row, 4).Value = item.NumeroIdentificacion;
            ws.Cell(row, 5).Value = item.Area ?? "";
            ws.Cell(row, 6).Value = item.Motivo;
            ws.Cell(row, 7).Value = item.FechaEntrada.ToString("HH:mm");
            ws.Cell(row, 8).Value = item.FechaSalida?.ToString("HH:mm") ?? "—";
            ws.Cell(row, 9).Value = item.MinutosEstancia?.ToString() ?? "—";
            ws.Cell(row, 10).Value = item.EstadoAcceso;
            ws.Cell(row, 11).Value = item.CodigoGafete ?? "—";
            ws.Cell(row, 12).Value = item.Guardia;

            var color = item.EstadoAcceso switch
            {
                "Aprobado" or "Finalizado" => XLColor.FromHtml("#F0FDF4"),
                "Rechazado" => XLColor.FromHtml("#FEF2F2"),
                _ => XLColor.FromHtml("#FFFBEB")
            };
            ws.Row(row).Style.Fill.BackgroundColor = color;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Exportar PDF ───────────────────────────────────────────────────
    public async Task<byte[]> ExportarPdfHoyAsync()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var hoy = DateTime.UtcNow.Date;
        var (items, total) = await ObtenerHistorialAsync(null, null, hoy, hoy, 1, int.MaxValue);
        var lista = items.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(9));

                static IContainer HeaderCellStyle(IContainer container) =>
                    container.Background("#1E293B").Padding(4).AlignCenter();

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("ROCLAND — Control de Acceso")
                                .FontSize(16).Bold().FontColor("#1E293B");
                            c.Item().Text($"Reporte del día: {hoy:dd/MM/yyyy}  |  Total: {total} accesos")
                                .FontSize(10).FontColor("#64748B");
                        });
                        row.ConstantItem(80).AlignRight()
                            .Text(DateTime.Now.ToString("HH:mm"))
                            .FontSize(10).FontColor("#94A3B8");
                    });
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor("#E2E8F0");
                });

                page.Content().PaddingTop(12).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(60);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                        cols.ConstantColumn(45);
                        cols.ConstantColumn(45);
                        cols.ConstantColumn(40);
                        cols.ConstantColumn(60);
                        cols.ConstantColumn(40);
                    });

                    table.Header(header =>
                    {
                        var headerStyle = new TextStyle().FontColor(Colors.White).Bold().FontSize(8);

                        header.Cell().Element(HeaderCellStyle).Text("Tipo").Style(headerStyle);
                        header.Cell().Element(HeaderCellStyle).Text("Nombre").Style(headerStyle);
                        header.Cell().Element(HeaderCellStyle).Text("Empresa").Style(headerStyle);
                        header.Cell().Element(HeaderCellStyle).Text("No. ID").Style(headerStyle);
                        header.Cell().Element(HeaderCellStyle).Text("Área").Style(headerStyle);
                        header.Cell().Element(HeaderCellStyle).Text("Motivo").Style(headerStyle);
                        header.Cell().Element(HeaderCellStyle).Text("Entrada").Style(headerStyle);
                        header.Cell().Element(HeaderCellStyle).Text("Salida").Style(headerStyle);
                        header.Cell().Element(HeaderCellStyle).Text("Min").Style(headerStyle);
                        header.Cell().Element(HeaderCellStyle).Text("Estado").Style(headerStyle);
                        header.Cell().Element(HeaderCellStyle).Text("Gafete").Style(headerStyle);
                    });

                    bool alt = false;
                    foreach (var item in lista)
                    {
                        var bg = alt ? "#F8FAFC" : "#FFFFFF";
                        alt = !alt;

                        var estadoColor = item.EstadoAcceso switch
                        {
                            "Aprobado" or "Finalizado" => "#16A34A",
                            "Rechazado" => "#DC2626",
                            _ => "#D97706"
                        };

                        void Cell(string text, string? color = null)
                        {
                            table.Cell().Background(bg).Padding(4)
                                .Text(text).FontColor(color ?? "#1E293B").FontSize(8);
                        }

                        Cell(item.Tipo, item.Tipo == "Visitante" ? "#2563EB" : "#7C3AED");
                        Cell(item.Nombre);
                        Cell(item.Empresa ?? "—");
                        Cell(item.NumeroIdentificacion);
                        Cell(item.Area ?? "—");
                        Cell(item.Motivo);
                        Cell(item.FechaEntrada.ToString("HH:mm"));
                        Cell(item.FechaSalida?.ToString("HH:mm") ?? "—");
                        Cell(item.MinutosEstancia?.ToString() ?? "—");
                        Cell(item.EstadoAcceso, estadoColor);
                        Cell(item.CodigoGafete ?? "—");
                    }
                });

                page.Footer().AlignCenter()
                    .Text(t =>
                    {
                        t.Span("Rocland AccesoControl  ·  Página ").FontColor("#94A3B8");
                        t.CurrentPageNumber().FontColor("#94A3B8");
                        t.Span(" de ").FontColor("#94A3B8");
                        t.TotalPages().FontColor("#94A3B8");
                    });
            });
        });

        return document.GeneratePdf();
    }
}