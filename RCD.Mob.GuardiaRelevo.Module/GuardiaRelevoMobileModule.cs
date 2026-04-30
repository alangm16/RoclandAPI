using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using RCD.Mob.GuardiaRelevo.Infrastructure;
using RCD.Mob.GuardiaRelevo.Infrastructure.Services;
using RCD.Mob.GuardiaRelevo.Web.Controllers;
using RCD.Shared.Kernel.Modularity;

namespace RCD.Mob.GuardiaRelevo.Module;

public class GuardiaRelevoMobileModule : IRoclandModule
{
    public string Name => "GuardiaRelevoMobile";
    public string Version => "1.0.0";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Registrar controllers del módulo
        services.AddControllers()
            .AddApplicationPart(typeof(AuthController).Assembly);

        // Registrar infraestructura del módulo
        services.AddGuardiaRelevoModule(configuration);
        //services.AddScoped<IAuthService, AuthService>();
    }

    public void ConfigureApplication(IApplicationBuilder app)
    {
        // Sin hubs SignalR ni middlewares específicos por ahora
    }
}