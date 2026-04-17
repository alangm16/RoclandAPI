using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RCD.Shared.Kernel.Modularity
{
    public interface IRoclandModule
    {
        // 1. Identificación del módulo
        string Name { get; }
        string Version { get; }

        // 2. Fase de Inyección de Dependencias (IoC)
        // Aquí el módulo registrará sus propios DbContexts, Servicios, etc.
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);

        // 3. Fase de Configuración del Pipeline HTTP
        // Aquí el módulo registrará sus middlewares, endpoints, SignalR hubs, etc.
        void ConfigureApplication(IApplicationBuilder app);
    }
}