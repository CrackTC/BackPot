using BackPot.Server.Authorization;
using BackPot.Server.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BackPot.Server.Services;

class BackPotService
{
    private readonly BackPotServerOptions _options;
    private readonly ITokenValidation _validation;
    private readonly Dictionary<string, Backup> _backups;
    private string JsonPath => Path.Combine(_options.BackupRoot, "backpot.json");

    public async Task<IResult> Upload(string token, string name, IFormFileCollection files, ILogger<Backup> logger)
    {
        if (!_validation.IsValidToken(token))
            return Results.Unauthorized();
        if (!_backups.TryGetValue(name, out Backup? backup))
        {
            backup = new(name, _options.MaxGenerations);
            _backups[name] = backup;
        }
        await backup.NewGeneration(files, logger, _options.BackupRoot);
        Save();
        return Results.Ok(backup);
    }

    public IResult SetMaxGenerations(string token, string name, int maxGenerations)
    {
        if (!_validation.IsValidToken(token))
            return Results.Unauthorized();
        if (maxGenerations < 1)
            return Results.BadRequest("Max generations must be at least 1");
        if (_backups.TryGetValue(name, out Backup? backup))
        {
            backup.MaxGenerations = maxGenerations;
            Save();
            return Results.Ok(backup);
        }
        return Results.NotFound();
    }

    public IResult GetGenerations(string token, string name)
    {
        if (!_validation.IsValidToken(token))
            return Results.Unauthorized();
        if (_backups.TryGetValue(name, out Backup? backup))
            return Results.Ok(backup.Generations.Reverse());
        return Results.NotFound();
    }

    public IResult GetBackups(string token) => _validation.IsValidToken(token) ? Results.Ok(_backups.Values) : Results.Unauthorized();
    public IResult GetBackup(string token, string name)
    {
        if (!_validation.IsValidToken(token))
            return Results.Unauthorized();
        if (_backups.TryGetValue(name, out Backup? backup))
            return Results.Ok(backup);
        return Results.NotFound();
    }

    public IResult Remove(string token, string name)
    {
        if (!_validation.IsValidToken(token))
            return Results.Unauthorized();
        if (_backups.TryGetValue(name, out Backup? backup))
        {
            _backups.Remove(name);
            backup.Delete(_options.BackupRoot);
            Save();
            return Results.Ok(true);
        }
        return Results.NotFound();
    }

    public IResult ListFiles(string token, string name, ILogger<Backup> logger, int generation = 0)
    {
        if (!_validation.IsValidToken(token))
            return Results.Unauthorized();
        if (_backups.TryGetValue(name, out Backup? backup))
            return Results.Ok(backup.Ls(generation, logger));
        return Results.NotFound();
    }

    public IResult GetFile(string token, string name, string file, ILogger<Backup> logger, int generation = 0)
    {
        if (!_validation.IsValidToken(token))
            return Results.Unauthorized();
        if (_backups.TryGetValue(name, out Backup? backup))
            return backup.GetFile(file, generation, logger);
        return Results.NotFound();
    }

    public void Save() => File.WriteAllText(JsonPath, JsonSerializer.Serialize(_backups));

    public BackPotService(IOptions<BackPotServerOptions> options, ILogger<BackPotService> logger, ITokenValidation validation)
    {
        _options = options.Value;
        _validation = validation;
        logger.LogInformation("Backup root: {BackupRoot}", _options.BackupRoot);
        logger.LogInformation("Token: {Token}", _options.Token);
        _backups = File.Exists(JsonPath)
                      ? JsonSerializer.Deserialize<Dictionary<string, Backup>>(File.ReadAllText(JsonPath)) ?? []
                      : [];
    }
}
