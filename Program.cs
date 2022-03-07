using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Http;

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
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
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
            app.MapPost("/settings", (DuengerValues newValues, ILogger<Program> log) =>
            {
                log.LogInformation($"/settings(POST): new duenger values:");
                foreach (var duenger in newValues.duengervalues )
                {
                    log.LogInformation($"  {duenger.ToString()}");
                }
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
