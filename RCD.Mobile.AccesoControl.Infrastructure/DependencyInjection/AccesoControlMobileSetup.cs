using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RCD.Mob.AccesoControl.Infrastructure.DependencyInjection;

public static class AccesoControlMobileSetup
{
    public static IServiceCollection AddAccesoControlMobileModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // No registra DbContext ni servicios propios —
        // los hereda del módulo Web que ya los registró.
        // Solo registra la policy exclusiva de guardias.
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AccesoControlMobilePolicy", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Guardia")
                      .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
        });

        return services;
    }
}