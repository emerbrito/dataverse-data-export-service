
using ConsoleApp;
using EmBrito.Dataverse.DataExport.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Model;
using System;
using System.Configuration;
using System.Diagnostics;

// Optimize Connection settings for the ServiceClient
System.Net.ServicePointManager.DefaultConnectionLimit = 65000;
System.Threading.ThreadPool.SetMinThreads(100, 100);
System.Net.ServicePointManager.Expect100Continue = false;
System.Net.ServicePointManager.UseNagleAlgorithm = false;

// Build a config object
IConfiguration config = new ConfigurationBuilder()
    .AddUserSecrets(typeof(Program).Assembly, optional: true)
    .AddJsonFile("settings.json", optional: true)
    .AddJsonFile("local.settings.json", optional: true)    
    .AddEnvironmentVariables()
    .Build();

// instantiate logger
ServiceProvider serviceProvider = new ServiceCollection()
    .AddOptions()
    .AddLogging((loggingBuilder) => loggingBuilder
        .SetMinimumLevel(LogLevel.Trace)
        .AddConsole()
        )    
    .Configure<ApplicationOptions>(opt => 
    {
        config.Bind(opt);
        opt.StoreConnectionString = config.GetConnectionString("StoreConnectionString")!;
    })
    .BuildServiceProvider();

var options = serviceProvider.GetService<IOptions<ApplicationOptions>>();
var logger = serviceProvider.GetService<ILoggerFactory>()!.CreateLogger<Program>();

var connectionOptions = new ConnectionOptions
{
    AuthenticationType = AuthenticationType.ClientSecret,
    ClientId = config.GetValue<string>("ClientId"),
    ClientSecret = config.GetValue<string>("ClientSecret"),
    Logger = logger,
    ServiceUri = new Uri("https://ebrito.crm.dynamics.com")
};

using (ServiceClient client = new ServiceClient(connectionOptions))
{
    Console.WriteLine("");
    Console.WriteLine($"Connected to: {client.ConnectedOrgFriendlyName}");
    Console.WriteLine($"Executing...");

    var stopWatch = new Stopwatch();
    stopWatch.Start();

    var des = new DataExport(client, config, logger);
    await des.Start();

    stopWatch.Stop();
    Console.WriteLine($"Elapsed time (ms): {stopWatch.ElapsedMilliseconds}.");
}
