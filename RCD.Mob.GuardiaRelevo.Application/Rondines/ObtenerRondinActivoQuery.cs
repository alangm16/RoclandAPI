using MediatR;
using RCD.Mob.GuardiaRelevo.Application.DTOs;

namespace RCD.Mob.GuardiaRelevo.Application.Rondines;

public record ObtenerRondinActivoQuery() : IRequest<RondinActivoDto?>;