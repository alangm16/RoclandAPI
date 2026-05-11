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
        // Política exclusiva para app móvil de guardias.
        // Exige token JWT válido y que el claim "nombreRol" sea "Guardia".
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AccesoControlMobilePolicy", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("nombreRol", "Guardia")  // desde SA
                      .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
        });

        return services;
    }
}