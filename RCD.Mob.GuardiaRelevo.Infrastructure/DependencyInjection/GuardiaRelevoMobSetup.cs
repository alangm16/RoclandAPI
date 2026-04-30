using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using RCD.Mob.GuardiaRelevo.Domain.Interfaces;
using RCD.Mob.GuardiaRelevo.Infrastructure.Data;
using RCD.Mob.GuardiaRelevo.Infrastructure.Repositories;
using RCD.Mob.GuardiaRelevo.Infrastructure.Services;

namespace RCD.Mob.GuardiaRelevo.Infrastructure;

public static class GuardiaRelevoSetup
{
    public static IServiceCollection AddGuardiaRelevoModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<GuardiaRelevoDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("GuardiaRelevoDb"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_GuardiaRelevo")
            )
        );

        // Repositorios
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRondinRepository, RondinRepository>();
        services.AddScoped<IChecklistRepository, ChecklistRepository>();

        // Servicios de infraestructura
        //services.AddScoped<JwtTokenService>();

        // Servicios de aplicación
        //services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRondinService, RondinService>();
        services.AddScoped<IChecklistService, ChecklistService>();

        return services;
    }
}