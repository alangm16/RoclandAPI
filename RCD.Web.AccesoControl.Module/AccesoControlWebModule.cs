using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RCD.Shared.Kernel.Modularity; // El namespace de tu nueva interfaz
using RCD.Web.AccesoControl.Infrastructure; // Para acceder a tu capa de infraestructura
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

        // Aquí en el futuro registrarás tus Handlers de eventos, servicios propios de la capa de aplicación, etc.
    }

    // 3. Configuración del Pipeline HTTP
    public void ConfigureApplication(IApplicationBuilder app)
    {
        // Mapear el Hub de SignalR de este módulo
        // La ruta /accesohub coincide con:
        //   - OnMessageReceived en Program.cs (busca "accesohub")
        //   - AppConstants.SignalRHubPath en la app móvil ("/accesohub")
        if (app is WebApplication webApp)
        {
            webApp.MapHub<AccesoControlHub>("/accesohub");
        }
    }
}