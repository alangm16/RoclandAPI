using RCD.Mob.GuardiaRelevo.Application.DTOs;

namespace RCD.Mob.GuardiaRelevo.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(string usuario, string password, CancellationToken ct = default);
}