namespace RCD.SuperAdmin.Application.DTOs.Dispositivos;

// forzar push
public record RegistrarDispositivoRequest(
    string? FcmToken,
    string? DeviceToken,
    string? DispositivoInfo
);