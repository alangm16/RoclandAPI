using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RCD.Mob.AccesoControl.Infrastructure.DependencyInjection;
using RCD.Mob.AccesoControl.Web.Controllers;
using RCD.Shared.Kernel.Modularity;

namespace RCD.Mob.AccesoControl.Infrastructure;

public class AccesoControlMobileModule : IRoclandModule
{
    public string Name => "AccesoControlMobile";
    public string Version => "1.0.0";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Registrar los controllers de este módulo
        services.AddControllers()
            .AddApplicationPart(
                typeof(Controllers.GuardiasController).Assembly);

        // Registrar policy Mobile
        services.AddAccesoControlMobileModule(configuration);
    }

    public void ConfigureApplication(IApplicationBuilder app)
    {
        // Este módulo no tiene Hub propio —
        // comparte el /accesohub registrado por el módulo Web.
    }
}