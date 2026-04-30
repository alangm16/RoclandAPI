using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RCD.Shared.Kernel.Modularity;
using RCD.SuperAdmin.Infrastructure.DependencyInjection;

namespace RCD.SuperAdmin.Module
{
    public class SuperAdminModule : IRoclandModule
    {
        public string Name => "SuperAdmin";
        public string Version => "1.0.0";

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(Web.Controllers.SuperAdminAuthController).Assembly);
            services.AddControllers()
                .AddApplicationPart(typeof(Web.Controllers.PermisosController).Assembly);

            services.AddSuperAdminInfrastructure(configuration);
        }

        public void ConfigureApplication(IApplicationBuilder app)
        {
            // Sin hubs SignalR ni middlewares específicos por ahora
        }
    }
}
