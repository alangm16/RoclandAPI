

using RCD.SuperAdmin.Application.DTOs;

namespace RCD.SuperAdmin.Application.Interfaces
{
    public interface ISuperAdminAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}
