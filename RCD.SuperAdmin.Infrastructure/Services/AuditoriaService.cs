using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs;
using RCD.SuperAdmin.Application.DTOs.Auditoria;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly SuperAdminDbContext _db;

    public AuditoriaService(SuperAdminDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AuditoriaDto>> ObtenerRegistrosAsync(FiltroAuditoriaDto filtro)
    {
        const string sql = """
            SELECT 'Usuario'               AS NombreEntidad,
                   'Usuario'               AS EntidadAfectada,
                   e.Id                    AS RegistroId,
                   'Creación'              AS Accion,
                   u.Username              AS UsuarioResponsable,
                   e.FechaCreacion         AS Fecha
            FROM TBL_ROCLAND_SUPERADMIN_USUARIOS e
            INNER JOIN TBL_ROCLAND_SUPERADMIN_USUARIOS u ON e.CreadoPor = u.Id
            WHERE e.CreadoPor IS NOT NULL

            UNION ALL

            SELECT 'Usuario'               AS NombreEntidad,
                   'Usuario'               AS EntidadAfectada,
                   e.Id                    AS RegistroId,
                   'Modificación'          AS Accion,
                   u.Username              AS UsuarioResponsable,
                   e.FechaModificacion     AS Fecha
            FROM TBL_ROCLAND_SUPERADMIN_USUARIOS e
            INNER JOIN TBL_ROCLAND_SUPERADMIN_USUARIOS u ON e.ModificadoPor = u.Id
            WHERE e.ModificadoPor IS NOT NULL
              AND e.FechaModificacion > e.FechaCreacion

            UNION ALL

            SELECT 'Proyecto'              AS NombreEntidad,
                   'Proyecto'              AS EntidadAfectada,
                   p.Id                    AS RegistroId,
                   'Creación'              AS Accion,
                   u.Username              AS UsuarioResponsable,
                   p.FechaCreacion         AS Fecha
            FROM TBL_ROCLAND_SUPERADMIN_PROYECTOS p
            INNER JOIN TBL_ROCLAND_SUPERADMIN_USUARIOS u ON p.CreadoPor = u.Id
            WHERE p.CreadoPor IS NOT NULL

            UNION ALL

            SELECT 'Proyecto'              AS NombreEntidad,
                   'Proyecto'              AS EntidadAfectada,
                   p.Id                    AS RegistroId,
                   'Modificación'          AS Accion,
                   u.Username              AS UsuarioResponsable,
                   p.FechaModificacion     AS Fecha
            FROM TBL_ROCLAND_SUPERADMIN_PROYECTOS p
            INNER JOIN TBL_ROCLAND_SUPERADMIN_USUARIOS u ON p.ModificadoPor = u.Id
            WHERE p.ModificadoPor IS NOT NULL
              AND p.FechaModificacion > p.FechaCreacion

            UNION ALL

            SELECT 'ProyectoUsuarioRol'    AS NombreEntidad,
                   'Asignación de Proyecto'AS EntidadAfectada,
                   pur.Id                  AS RegistroId,
                   'Creación'              AS Accion,
                   u.Username              AS UsuarioResponsable,
                   pur.FechaCreacion       AS Fecha
            FROM TBL_ROCLAND_SUPERADMIN_PROYECTO_USUARIO_ROL pur
            INNER JOIN TBL_ROCLAND_SUPERADMIN_USUARIOS u ON pur.CreadoPor = u.Id
            WHERE pur.CreadoPor IS NOT NULL

            UNION ALL

            SELECT 'ProyectoUsuarioRol'    AS NombreEntidad,
                   'Asignación de Proyecto'AS EntidadAfectada,
                   pur.Id                  AS RegistroId,
                   'Modificación'          AS Accion,
                   u.Username              AS UsuarioResponsable,
                   pur.FechaModificacion   AS Fecha
            FROM TBL_ROCLAND_SUPERADMIN_PROYECTO_USUARIO_ROL pur
            INNER JOIN TBL_ROCLAND_SUPERADMIN_USUARIOS u ON pur.ModificadoPor = u.Id
            WHERE pur.ModificadoPor IS NOT NULL
              AND pur.FechaModificacion > pur.FechaCreacion

            UNION ALL

            SELECT 'UsuarioVistaAcceso'    AS NombreEntidad,
                   'Visibilidad de Vista'  AS EntidadAfectada,
                   uva.Id                  AS RegistroId,
                   'Creación'              AS Accion,
                   u.Username              AS UsuarioResponsable,
                   uva.FechaCreacion       AS Fecha
            FROM TBL_ROCLAND_SUPERADMIN_USUARIO_VISTAS_ACCESO uva
            INNER JOIN TBL_ROCLAND_SUPERADMIN_USUARIOS u ON uva.CreadoPor = u.Id
            WHERE uva.CreadoPor IS NOT NULL

            UNION ALL

            SELECT 'UsuarioVistaAcceso'    AS NombreEntidad,
                   'Visibilidad de Vista'  AS EntidadAfectada,
                   uva.Id                  AS RegistroId,
                   'Modificación'          AS Accion,
                   u.Username              AS UsuarioResponsable,
                   uva.FechaModificacion   AS Fecha
            FROM TBL_ROCLAND_SUPERADMIN_USUARIO_VISTAS_ACCESO uva
            INNER JOIN TBL_ROCLAND_SUPERADMIN_USUARIOS u ON uva.ModificadoPor = u.Id
            WHERE uva.ModificadoPor IS NOT NULL
              AND uva.FechaModificacion > uva.FechaCreacion
            """;

        // Construcción dinámica de filtros
        var condiciones = new List<string>();
        var parametros = new List<object>();

        if (!string.IsNullOrWhiteSpace(filtro.Usuario))
        {
            condiciones.Add($"UsuarioResponsable LIKE {{{parametros.Count}}}");
            parametros.Add($"%{filtro.Usuario}%");
        }
        if (!string.IsNullOrWhiteSpace(filtro.Entidad))
        {
            condiciones.Add($"NombreEntidad = {{{parametros.Count}}}");
            parametros.Add(filtro.Entidad);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Accion))
        {
            condiciones.Add($"Accion = {{{parametros.Count}}}");
            parametros.Add(filtro.Accion);
        }
        if (filtro.Desde.HasValue)
        {
            condiciones.Add($"Fecha >= {{{parametros.Count}}}");
            parametros.Add(filtro.Desde.Value);
        }
        if (filtro.Hasta.HasValue)
        {
            condiciones.Add($"Fecha <= {{{parametros.Count}}}");
            parametros.Add(filtro.Hasta.Value);
        }

        var where = condiciones.Count > 0
            ? "WHERE " + string.Join(" AND ", condiciones)
            : string.Empty;

        // Consulta para contar el total
        var countSql = $"SELECT COUNT(*) FROM ({sql}) AS Auditoria {where}";
        var total = await _db.Database
            .SqlQueryRaw<int>(countSql, parametros.ToArray())
            .FirstOrDefaultAsync();

        // Consulta paginada
        var paginaSql = $"SELECT * FROM ({sql}) AS Auditoria {where} ORDER BY Fecha DESC OFFSET {((filtro.Pagina - 1) * filtro.TamanoPagina)} ROWS FETCH NEXT {filtro.TamanoPagina} ROWS ONLY";
        var items = await _db.Database
            .SqlQueryRaw<AuditoriaDto>(paginaSql, parametros.ToArray())
            .ToListAsync();

        return new PagedResult<AuditoriaDto>(
            items,
            total,
            filtro.Pagina,
            filtro.TamanoPagina
        );
    }
}