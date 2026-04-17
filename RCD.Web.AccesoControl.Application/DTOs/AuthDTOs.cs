namespace RCD.Web.AccesoControl.Application.DTOs;

public record LoginRequest(string Usuario, string Password);

public record LoginResponse(
    string Token,
    string Nombre,
    string Rol,           // "Guardia" | "Admin" | "Supervisor"
    int Id,
    DateTime Expiracion
);