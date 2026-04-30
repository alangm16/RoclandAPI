using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RCD.Shared.Kernel.Modularity; 
using RCD.Web.AccesoControl.Infrastructure; 
using RCD.Web.AccesoControl.Infrastructure.Hubs;
namespace RCD.Web.AccesoControl.Module;

public class AccesoControlWebModule : IRoclandModule
{
    // 1. Identificadores del módulo
    public string Name => "AccesoControlWeb";
    public string Version => "1.0.0";

    // 2. Inyección de dependencias (IoC) centralizada
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // A. Registrar los Controladores del módulo para que la API los descubra
        services.AddControllers()
            .AddApplicationPart(typeof(Web.Controllers.AdminController).Assembly);

        // B. Delegar la inyección a tu capa de infraestructura
        // (Esto llama al método AddAccesoControlWebModule que ya tienes en AccesoControlWebSetup.cs)
        services.AddAccesoControlWebModule(configuration);

    }

    // 3. Configuración del Pipeline HTTP
    public void ConfigureApplication(IApplicationBuilder app)
    {

        if (app is WebApplication webApp)
        {
            webApp.MapHub<AccesoControlHub>("/accesohub");
        }
    }
}