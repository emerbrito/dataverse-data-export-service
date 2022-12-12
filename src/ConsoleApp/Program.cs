
using ConsoleApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Model;
using System;

// Optimize Connection settings for the ServiceClient
System.Net.ServicePointManager.DefaultConnectionLimit = 65000;
System.Threading.ThreadPool.SetMinThreads(100, 100);
System.Net.ServicePointManager.Expect100Continue = false;
System.Net.ServicePointManager.UseNagleAlgorithm = false;

// Build a config object
IConfiguration config = new ConfigurationBuilder()
    .AddUserSecrets(typeof(Program).Assembly)
    .AddJsonFile("settings.json", optional: true)
    .AddJsonFile("local.settings.json", optional: true)    
    .AddEnvironmentVariables()
    .Build();

// instantiate logger
ServiceProvider serviceProvider = new ServiceCollection()
    .AddLogging((loggingBuilder) => loggingBuilder
        .SetMinimumLevel(LogLevel.Information)
        .AddConsole()
        )
    .BuildServiceProvider();

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

    var des = new DataExport(client, config, logger);
    await des.Start();
}