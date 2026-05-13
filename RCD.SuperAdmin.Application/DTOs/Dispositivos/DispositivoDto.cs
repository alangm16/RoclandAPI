using System.Security.Cryptography.X509Certificates;

namespace RCD.SuperAdmin.Application.DTOs.Dispositivos;

public record DispositivoDto(
    int Id,
    string Plataforma,
    string? DeviceToken,
    string? FcmToken,
    string? DispositivoInfo,
    bool Activo,
    DateTime FechaCreacion
);
