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

        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(
                builder => builder
                            .AddConsole()
                            .AddDebug()
                            .SetMinimumLevel(LogLevel.Debug)
            );
            var log = loggerFactory.CreateLogger<SignalProcessor>();

            ISignalReceiver signalReceiver;
            try
            {
                signalReceiver = new GpioSignalReceiver();
            }
            catch (Exception ex)
            {
                log.LogCritical(ex.Message);
                log.LogInformation("leider kein GPIO heute");
                signalReceiver = new DemoSignalReceiver();
            }

            var signalProcessor = new SignalProcessor( signalReceiver, TimeSpan.FromSeconds(20), log);
            Task signalTask = Task.Factory.StartNew( async () => 
            {
                while (true)
                {
                    signalProcessor.Refresh();
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });

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
                 return new { 
                     calculated = signalProcessor.KilosPerHektar, 
                     distance   = signalProcessor.OverallMeters, 
                     amount     = signalProcessor.OverallKilos, 
                     fertilizer = signalProcessor.CurrentName };
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
