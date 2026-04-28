using System.ComponentModel.DataAnnotations;

namespace RCD.Web.AccesoControl.Application.DTOs;

public record LoginRequest([Required]string Usuario, [Required]string Password);

public record LoginResponse(
    string Token,
    string Nombre,
    string Rol,           // "Guardia" | "Admin" | "Supervisor"
    int Id,
    DateTime Expiracion
);