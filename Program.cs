using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System;
using System.Threading.Tasks;

namespace Fertilizer
{
    class DuengerValues
    {
        public Duenger[]? duengervalues { get; set; }
    }
    class KindOfDuenger
    {
        public string? fertilizername { get; set;}
    }

    class Program
    {
        static readonly string DUENGER_CONFIG_FILENAME = "./conf/duenger.json";

        static ISignalReceiver CreateSignalReceiver(ILogger log)
        {
            ISignalReceiver signalReceiver;
            try
            {
                signalReceiver = new GpioSignalReceiver();
            }
            catch (Exception ex)
            {
                log.LogCritical(ex.Message);
                log.LogWarning("leider kein GPIO heute. Settings up FakeSignalGenerator");
                signalReceiver = new DemoSignalReceiver();
            }

            return signalReceiver;
        }

        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(
                builder => builder
                            .AddSimpleConsole( options => {
                                options.SingleLine = true;
                                options.IncludeScopes = false;
                                options.TimestampFormat = "hh:mm:ss ";
                            })
                            .AddDebug()
                            .SetMinimumLevel(LogLevel.Debug)
            );
            var log = loggerFactory.CreateLogger<SignalProcessor>();

            var signalReceiver = CreateSignalReceiver(log);
            var signalProcessor = new SignalProcessor( signalReceiver, TimeSpan.FromSeconds(3), log);
            signalProcessor.SetDuenger("Kali", 30, 6.1f);
            Task refreshingTask = signalProcessor.StartRefresh();

            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddSimpleConsole( options => {
                options.SingleLine = true;
                options.IncludeScopes = false;
                options.TimestampFormat = "hh:mm:ss ";
            });
            //builder.Services.AddControllers();
            var app = builder.Build();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            //app.MapControllers();
            app.Urls.Add("http://*:8080");

            app.MapGet("/settings", (ILogger<Program> log) =>
            {
                string settingsFromFile = File.ReadAllText(DUENGER_CONFIG_FILENAME);
                log.LogInformation($"/settings(GET): {settingsFromFile}");
                return settingsFromFile;
            });

            app.MapGet("/calculate", (ILogger<Program> log) =>
            {
                 var data = new { 
                     kilosperhektar = signalProcessor.KilosPerHektar, 
                     overallmeters  = signalProcessor.OverallMeters, 
                     overallkilos   = signalProcessor.OverallKilos, 
                     fertilizername = signalProcessor.CurrentName };
                log.LogInformation($"/calculate(GET): {data.ToString()}");
                return data;
            });

            app.MapPost("/settings", (DuengerValues newValues, ILogger<Program> log) =>
            {
                ArgumentNullException.ThrowIfNull(newValues.duengervalues);

                log.LogInformation($"/settings(POST): new duenger values:");
                foreach (var duenger in newValues.duengervalues )
                {
                    log.LogInformation($"  {duenger.Kg,-8:###.###} {duenger.Name}");
                }

                string jsonString = JsonSerializer.Serialize(newValues.duengervalues);
                log.LogInformation($"/settings(POST): new duenger values as JSON: >>>{jsonString}<<<");
                File.WriteAllText(DUENGER_CONFIG_FILENAME, jsonString);

                return Results.Ok();
            });

            app.MapPost("/applyChanges", (KindOfDuenger kind, ILogger<Program> log) => 
            {
                log.LogInformation($"/applyChanges: set fertilizer to [{kind.fertilizername}]");
                return Results.Ok();
            });

            app.Run();
        }
    }
}
