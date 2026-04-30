using RCD.Shared.Kernel.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RCD.SuperAdmin.Module
{
    public class SuperAdminModule : IRoclandModule
    {
        public string Name => "SuperAdmin";
        public string Version => "1.0.0";

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(Web.Controllers.SuperAdminController).Assembly);

            services.AddSuperAdminModule(configuration);
        }
    }
}
