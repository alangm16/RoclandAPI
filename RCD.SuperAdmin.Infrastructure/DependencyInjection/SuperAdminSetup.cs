using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RCD.Shared.Infrastructure.Notifications;
using RCD.Shared.Kernel.Interfaces;
using RCD.Shared.Kernel.Settings;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Infrastructure.Data;
using RCD.SuperAdmin.Infrastructure.Notifications;
using RCD.SuperAdmin.Infrastructure.Services;

namespace RCD.SuperAdmin.Infrastructure.DependencyInjection;

public static class SuperAdminSetup
{
    public static IServiceCollection AddSuperAdminInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();

        // ── DbContext ─────────────────────────────────────────────────────────
        services.AddDbContext<SuperAdminDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("SuperAdminConnection")));

        // ── JWT Settings ──────────────────────────────────────────────────────
        var jwtSection = configuration.GetSection("Jwt");
        services.Configure<JwtSettings>(jwtSection);
        var jwtSettings = jwtSection.Get<JwtSettings>()!;

        // ── Servicios de Infrastructure ───────────────────────────────────────
        services.AddHttpContextAccessor();                             
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
        services.AddScoped<IFcmTokenRepository, SuperAdminFcmTokenRepository>();
        services.AddHttpClient<IFcmService, FcmService>();

        // ── Servicios de Application ──────────────────────────────────────────
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IProyectoService, ProyectoService>();

        return services;
    }
}