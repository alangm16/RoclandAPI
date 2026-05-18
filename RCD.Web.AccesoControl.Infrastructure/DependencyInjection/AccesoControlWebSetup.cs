using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RCD.Web.AccesoControl.Application.Interfaces;
using RCD.Web.AccesoControl.Infrastructure.Data;
using RCD.Web.AccesoControl.Infrastructure.Services;
using RCD.Web.AccesoControl.Web.Services;
using RCD.Web.AccesoControl.Infrastructure.Settings;

namespace RCD.Web.AccesoControl.Infrastructure
{
    public static class AccesoControlWebSetup
    {
        public static IServiceCollection AddAccesoControlWebModule(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Registrar el interceptor
            services.AddScoped<AccesoControlAuditInterceptor>();

            // 2. Base de Datos configurada con el interceptor
            services.AddDbContext<AccesoControlWebDbContext>((sp, options) =>
            {
                var interceptor = sp.GetRequiredService<AccesoControlAuditInterceptor>();
                options.UseSqlServer(configuration.GetConnectionString("AccesoControlDb"))
                       .AddInterceptors(interceptor);
            });

            // 2. Mapeo de configuraciones del módulo
            // Esto permite inyectar IOptions<AccesoControlConfig> en tus servicios
            var moduleConfig = configuration.GetSection("Modules:AccesoControl");

            // 3. Registro de Servicios
            services.AddScoped<IAccesoService, AccesoService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IAuthService, AuthService>(); 
            services.AddScoped<ICatalogoService, CatalogoService>();    

            // 4. Background Services (si los tienes)
            services.AddHostedService<CierreAutomaticoService>();

            services.AddMemoryCache();

            return services;
        }
    }
}