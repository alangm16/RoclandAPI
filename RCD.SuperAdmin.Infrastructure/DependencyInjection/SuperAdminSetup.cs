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
        services.AddScoped<IPerfilFcmTokenProvider, PerfilFcmTokenProvider>();
        services.AddHttpClient<IFcmService, FcmService>();


        // ── Servicios de Application ──────────────────────────────────────────
        services.AddScoped<IAlertaService, AlertaService>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IConfiguracionService, ConfiguracionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDelegacionService, DelegacionService>();
        services.AddScoped<ILogAccesoService, LogAccesoService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IProyectoService, ProyectoService>();
        services.AddScoped<ITokenDispositivoService, TokenDispositivoService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IVisibilidadService, VisibilidadService>();
        services.AddScoped<ISesionService, SesionService>();

        return services;
    }
}