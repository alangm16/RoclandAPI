using MediatR;
using RCD.Mob.GuardiaRelevo.Application.DTOs;

namespace RCD.Mob.GuardiaRelevo.Application.Rondines;

public record ValidarQRCommand(
    int RondinId,
    string QRCode,
    string TipoGuardia   // "Saliente" | "Entrante"
) : IRequest<ValidarQRResultDto>;

public record ValidarQRResultDto(bool Exitoso, string Mensaje, int? UsuarioId = null);