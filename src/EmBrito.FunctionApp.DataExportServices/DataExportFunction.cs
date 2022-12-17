using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EmBrito.Dataverse.DataExport;
using EmBrito.Dataverse.DataExport.Core;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmBrito.FunctionApp.DataExportServices
{
    public class DataExportFunction
    {

        ILogger<DataExportFunction> logger;
        TelemetryClient telemetryClient; 
        ApplicationOptions options;
        DataExport dataExport;

        public DataExportFunction(DataExport dataExport, IOptions<ApplicationOptions> options, TelemetryClient telemetryClient, ILogger<DataExportFunction> logger)
        {
            this.logger = logger;
            this.telemetryClient = telemetryClient;
            this.options = options.Value;
            this.dataExport = dataExport;
        }

        [FunctionName("DataExportFunction")]
        public async Task Run(
            [TimerTrigger("%ScheduleCronExpression%", RunOnStartup = true)] TimerInfo timer,            
            CancellationToken cancelationToken)
        {

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            logger.LogInformation($"Starting data export.");
            await dataExport.Start(cancelationToken);
            logger.LogInformation($"Data export complete.");
            stopWatch.Stop();

            logger.LogInformation($"Elapsed time (ms): {stopWatch.Elapsed}.");

        }

    }
}
