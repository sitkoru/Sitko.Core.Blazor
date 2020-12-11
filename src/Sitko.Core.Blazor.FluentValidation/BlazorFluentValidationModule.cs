using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sitko.Core.App;
using Sitko.Core.App.Web;

namespace Sitko.Core.Blazor.FluentValidation
{
    public class BlazorFluentValidationModule : BaseApplicationModule<BlazorFluentValidationModuleConfig>,
        IWebApplicationModule
    {
        public BlazorFluentValidationModule(BlazorFluentValidationModuleConfig config,
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

    public class BlazorFluentValidationModuleConfig
    {
        public List<string> Namespaces { get; set; } = new List<string>();
    }
}
