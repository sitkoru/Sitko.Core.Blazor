using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sitko.Core.App;
using Sitko.Core.App.Web;

namespace Sitko.Core.Blazor.FluentValidation
{
    public class BlazorFluentValidationModule : BaseApplicationModule, IWebApplicationModule
    {
        public BlazorFluentValidationModule(BaseApplicationModuleConfig config,
            Application application) : base(config, application)
        {
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddScoped<FluentValidator>();
        }
    }
}
