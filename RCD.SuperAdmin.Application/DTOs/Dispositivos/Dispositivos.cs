namespace RCD.SuperAdmin.Application.DTOs.Dispositivos;

public record RegistrarDispositivoRequest(
    string? FcmToken,
    string? DeviceToken,
    string? DispositivoInfo
);