// LoginRequest.cs
namespace RCD.SuperAdmin.Application.DTOs.Auth;

public record LoginRequest(
    string Username,
    string Password,
    string? Plataforma = "Web",       // "Web" | "Mobile" | "Desktop"
    string? DispositivoInfo = null     // para logs
);