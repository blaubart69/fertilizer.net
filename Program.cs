using System.Text;
using System.Security.AccessControl;
using System.ComponentModel;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fertilizer
{
    class Duenger
    {
        [JsonPropertyName("name")]
        public string? Name {get; set; }
        [JsonPropertyName("kg")]
        public float? Kg {get; set; }

        public override string ToString()
        {
            return $"Name: {Name} Kg: {Kg}";
        }
    }

    class DuengerValues
    {
        public Duenger[]? duengervalues { get; set; }
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
            /*
            app.MapPost("/settings", async (HttpContext context, ILogger<Program> log) =>
            {
                string body;
                using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    body = await reader.ReadToEndAsync();
                }
                log.LogInformation($"/settings(POST): body: {body}");

                return Results.Ok();
            });
            */
            app.MapPost("/settings", (DuengerValues newValues, ILogger<Program> log) =>
            {
                log.LogInformation($"/settings(POST): new duenger values:");
                foreach (var duenger in newValues.duengervalues )
                {
                    log.LogInformation($"  {duenger.ToString()}");
                }
                return Results.Ok();
            });

            app.Run();
        }
    }
}
