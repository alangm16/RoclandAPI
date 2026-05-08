namespace RCD.SuperAdmin.Application.DTOs.Dashboard;

public class SuperAdminDashboardKpisDto
{
    public int UsuariosActivos { get; set; }
    public int UsuariosConectados { get; set; } // Opcional por ahora (SignalR)
    public int ModulosTotales { get; set; }
    public int AccesosDiarios { get; set; }
    public int AlertasAbiertas { get; set; }
    public int NuevosUsuariosMes { get; set; }
}

public class AccesosDiaSemanaDto
{
    public string Dia { get; set; } = string.Empty;
    public int Exitosos { get; set; }
    public int Fallidos { get; set; }
}

public class ModuloUsageDto
{
    public int ProyectoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Plataforma { get; set; } = string.Empty;
    public string IconoCss { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int UsuariosActivos { get; set; }
}

// AlertaSA y AccesoLog puedes ponerlos aquí o reusar los que ya tengas.

public class AlertaDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public bool Resuelta { get; set; }
    public string? AccionUrl { get; set; }
}

public class AccesoLogDto
{
    public int? UsuarioId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool Exitoso { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Plataforma { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}