using BackPot.Server.Authorization;
using BackPot.Server.Models;
using BackPot.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackPot.Server;

internal abstract class Program
{
    public static ILogger Logger { get; private set; } = null!;
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddConsole();
        builder.Configuration.AddEnvironmentVariables();
        builder.Services.Configure<BackPotServerOptions>(builder.Configuration);
        builder.Services.AddSingleton<ITokenValidation, TokenValidation>();
        builder.Services.AddSingleton<BackPotService>();

        var app = builder.Build();
        Logger = app.Logger;

        app.MapGet(
            "/backups",
            (BackPotService service, [FromHeader(Name = "x-token")] string token)
                => service.GetBackups(token)
        );

        app.MapGet(
            "/backups/{name:regex(^[a-zA-Z0-9_ -]+$)}",
            (string name, BackPotService service, [FromHeader(Name = "x-token")] string token)
                => service.GetBackup(token, name)
        );

        app.MapDelete(
            "/backups/{name:regex(^[a-zA-Z0-9_ -]+$)}",
            (string name, BackPotService service, [FromHeader(Name = "x-token")] string token)
                => service.Remove(token, name)
        );

        app.MapPost(
            "/backups/{name:regex(^[a-zA-Z0-9_ -]+$)}",
            async (string name,
             BackPotService service,
             [FromHeader(Name = "x-token")] string token,
             [FromForm] IFormFileCollection files,
             ILogger<Backup> logger)
                => await service.Upload(token, name, files, logger)
        ).DisableAntiforgery();

        app.MapPost(
            "/backups/{name:regex(^[a-zA-Z0-9_ -]+$)}/max_generations",
            (string name,
             BackPotService service,
             [FromHeader(Name = "x-token")] string token,
             [FromForm] int maxGenerations)
                => service.SetMaxGenerations(token, name, maxGenerations)
        );

        app.MapGet(
            "/backups/{name:regex(^[a-zA-Z0-9_ -]+$)}/generations",
            (string name, BackPotService service, [FromHeader(Name = "x-token")] string token)
                => service.GetGenerations(token, name)
        );

        app.MapGet(
            "/backups/{name:regex(^[a-zA-Z0-9_ -]+$)}/generations/{generation}",
            (string name,
             int generation,
             BackPotService service,
             [FromHeader(Name = "x-token")] string token,
             ILogger<Backup> logger)
                => service.ListFiles(token, name, logger, generation)
        );

        app.MapGet(
            "/backups/{name:regex(^[a-zA-Z0-9_ -]+$)}/generations/latest",
            (string name,
             BackPotService service,
             [FromHeader(Name = "x-token")] string token,
             ILogger<Backup> logger)
                => service.ListFiles(token, name, logger)
        );

        app.MapGet(
            "/backups/{name:regex(^[a-zA-Z0-9_ -]+$)}/generations/{generation}/{*file}",
            (string name,
             int generation,
             string file,
             BackPotService service,
             [FromHeader(Name = "x-token")] string token,
             ILogger<Backup> logger)
                => service.GetFile(token, name, file, logger, generation)
        );

        app.Run($"http://*:{app.Configuration.GetValue<int>("PORT")}/");
    }
}
