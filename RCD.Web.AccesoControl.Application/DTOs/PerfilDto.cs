namespace RCD.Web.AccesoControl.Application.DTOs
{
    public class PerfilDto
    {
        public int Id { get; set; }
        public int SuperAdminUsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? NumeroEmpleado { get; set; }
        public string? Turno { get; set; }
        public bool Activo { get; set; }
    }

    public class UsuarioSinPerfilDto
    {
        public int SuperAdminUsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string RolEnProyecto { get; set; } = string.Empty;
        public int NivelRol { get; set; }
    }

    // CrearPerfilRequest.cs
    public class CrearPerfilRequest
    {
        public int SuperAdminUsuarioId { get; set; }
        public string? NumeroEmpleado { get; set; }
        public string? Turno { get; set; }
        public bool Activo { get; set; }
    }

}