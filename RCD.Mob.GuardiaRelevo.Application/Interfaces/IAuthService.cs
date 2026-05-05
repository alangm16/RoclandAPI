using RCD.Mob.GuardiaRelevo.Application.DTOs;
using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Application.Interfaces;

public interface IAuthService
{
    Task<Usuario?> ObtenerPerfilPorSuperAdminIdAsync(int superAdminId);
    Task<int?> ObtenerSuperAdminIdPorQRAsync(string qrCode);
}