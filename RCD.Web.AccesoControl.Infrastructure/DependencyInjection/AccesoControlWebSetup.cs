using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RCD.Web.AccesoControl.Application.Interfaces;
using RCD.Web.AccesoControl.Infrastructure.Persistence;
using RCD.Web.AccesoControl.Infrastructure.Services;
using RCD.Web.AccesoControl.Web.Services;

namespace RCD.Web.AccesoControl.Infrastructure
{
    public static class AccesoControlWebSetup
    {
        public static IServiceCollection AddAccesoControlWebModule(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Base de Datos específica del módulo
            services.AddDbContext<AccesoControlWebDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("AccesoControlDb")));

            // 2. Mapeo de configuraciones del módulo
            // Esto permite inyectar IOptions<AccesoControlConfig> en tus servicios
            var moduleConfig = configuration.GetSection("Modules:AccesoControl");

            // 3. Registro de Servicios
            services.AddScoped<IAccesoService, AccesoService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IAuthService, AuthService>(); // El AuthService que ya tienes
            services.AddHttpClient<IFcmService, FcmService>();
            services.AddScoped<ICatalogoService, CatalogoService>();    

            // 4. Background Services (si los tienes)
            services.AddHostedService<CierreAutomaticoService>();

            return services;
        }
    }
}