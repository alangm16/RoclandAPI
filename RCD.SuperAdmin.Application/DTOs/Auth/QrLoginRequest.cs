namespace RCD.SuperAdmin.Application.DTOs.Auth;

public class QrLoginRequest
{
    public string QrCode { get; set; } = string.Empty;
    public string? Plataforma { get; set; } // Agregamos esto
}