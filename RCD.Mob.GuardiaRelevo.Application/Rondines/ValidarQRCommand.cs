using MediatR;
using RCD.Mob.GuardiaRelevo.Application.DTOs; 
namespace RCD.Mob.GuardiaRelevo.Application.Rondines;

// El DTO de respuesta
public record ValidarQRResultDto(bool Exito, string Mensaje, int? UsuarioId = null);

public record ValidarQRCommand(
    int RelevoId,
    string QRCode,
    string TipoGuardia // "Saliente" o "Entrante"
) : IRequest<ValidarQRResultDto>;