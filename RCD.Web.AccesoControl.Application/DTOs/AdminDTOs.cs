using System.ComponentModel.DataAnnotations;

namespace RCD.Web.AccesoControl.Application.DTOs;

// ==========================================
// ── KPIs & DASHBOARD ──
// ==========================================
public record DashboardKpiDto(
    int DentroAhora,
    int AccesosHoy,
    int VisitantesHoy,
    int ProveedoresHoy,
    double TiempoPromedio,
    int SolicitudesPendientes
);

public record FlujoPorHoraDto(
    int Hora,
    int Total
);

public record FlujoDiarioDto(
    string Fecha,
    int Visitantes,
    int Proveedores
);

public record AreaVisitadaDto(
    string Area,
    int Total
);

// ==========================================
// ── HISTORIAL ──
// ==========================================
public record HistorialAccesoDto(
    int Id,
    string Tipo,
    string Nombre,
    string? Empresa,
    string NumeroIdentificacion,
    string? Area,
    string Motivo,
    DateTime FechaEntrada,
    DateTime? FechaSalida,
    int? MinutosEstancia,
    string EstadoAcceso,
    string? CodigoGafete,
    string Guardia // Mantenemos el nombre de la propiedad, aunque ahora guarda el NombreCompleto del Perfil
);

// ==========================================
// ── PERSONAS ──
// ==========================================
public record PersonaPerfilDto(
    int Id,
    string Nombre,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string? Empresa,
    string? Telefono,
    string? Email,
    int TotalVisitas,
    DateTime FechaRegistro,
    DateTime? FechaUltimaVisita
);

// ==========================================
// ── CATÁLOGOS ──
// ==========================================
public record CatalogoCreateDto(
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Debe tener entre 2 y 100 caracteres")]
    string Nombre
);

public record AreaDto(int Id, string Nombre, bool Activo);
public record MotivoDto(int Id, string Nombre, bool Activo);
public record TipoIdDto(int Id, string Nombre, bool Activo);

// ==========================================
// ── GUARDIAS (Ahora Perfiles Operativos) ──
// ==========================================
public record GuardiaListDto(
    int Id,
    string Nombre,
    string Usuario, // Aquí ahora estamos enviando el Número de Empleado o el Tipo de Perfil
    bool Activo,
    DateTime FechaCreacion
);

public record GuardiaUpdateDto(
    bool Activo
);