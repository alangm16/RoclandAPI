using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Infrastructure.Data;
using RCD.SuperAdmin.Infrastructure.Services;
using RCD.Shared.Infrastructure.Security;

namespace RCD.SuperAdmin.Infrastructure.DependencyInjection;

public static class SuperAdminSetup
{
    public static IServiceCollection AddSuperAdminInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContextPool<SuperAdminDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SuperAdminConnection")),
            poolSize: 128);

        // Services
        services.AddScoped<ISuperAdminAuthService, SuperAdminAuthService>();
        services.AddScoped<IPermisosService, PermisosService>();
        // JWT
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        return services;
    }
}
