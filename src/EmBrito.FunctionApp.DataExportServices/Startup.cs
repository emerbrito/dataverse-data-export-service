using EmBrito.Dataverse.DataExport;
using EmBrito.Dataverse.DataExport.Core;
using EmBrito.FunctionApp.DataExportServices;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Startup))]
namespace EmBrito.FunctionApp.DataExportServices
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            
            var config = builder.GetContext().Configuration;

            builder.Services
                .AddOptions()
                .AddLogging()
                .Configure<ApplicationOptions>(opt =>
                 {
                     config.Bind(opt);
                     opt.StoreConnectionString = config.GetConnectionString("StoreConnectionString")!;
                 })
                .AddTransient<ServiceClient>(serviceProvider => 
                {
                    var options = serviceProvider.GetService<IOptions<ApplicationOptions>>();
                    var logger = serviceProvider.GetService<ILoggerFactory>()!.CreateLogger<ServiceClient>();
                    var clientOptions = new ConfigurationOptions
                    {
                        EnableAffinityCookie = false,
                    };
                    var connOptions = new ConnectionOptions
                    {
                        AuthenticationType = AuthenticationType.ClientSecret,
                        ClientId = options.Value.ClientId,
                        ClientSecret = options.Value.ClientSecret,
                        Logger = logger,
                        ServiceUri = new Uri(options.Value.DataverseInstanceUrl),
                    };

                    return new ServiceClient(connOptions, deferConnection: true, serviceClientConfiguration: clientOptions);
                })
                .AddTransient<DataExport>()
                .BuildServiceProvider();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {

            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("local.settings.json", optional: true)
                .AddUserSecrets(typeof(Startup).Assembly, optional: true)
                .AddEnvironmentVariables()
                .Build();
               
        }

    }
}
