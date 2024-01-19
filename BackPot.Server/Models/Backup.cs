using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace BackPot.Server.Models;

[Serializable]
public class Backup(string name, int maxGenerations)
{
    [JsonPropertyName("name")]
    public string Name { get; } = name;

    private int _maxGenerations = maxGenerations;

    [JsonPropertyName("max_generations")]
    public int MaxGenerations
    {
        get => _maxGenerations;
        set
        {
            _maxGenerations = value;
            while (Generations.Count > _maxGenerations)
            {
                var first = Generations.First!.Value.Path;
                Generations.RemoveFirst();
                if (Directory.Exists(first)) Directory.Delete(first, true);
            }
        }
    }

    [JsonPropertyName("generations")]
    public LinkedList<Generation> Generations = new();

    private static bool IsValidName(string name) =>
        !string.IsNullOrWhiteSpace(name) &&
        !name.Contains("..") &&
        name[0] is not '/' and not '\\' &&
        !Path.GetInvalidPathChars().Any(name.Contains);

    public async Task NewGeneration(IFormFileCollection files, ILogger<Backup> logger, string backupRoot)
    {
        string path = Path.Combine(backupRoot, Name, Guid.NewGuid().ToString());
        string? first = null;
        Generations.AddLast(new Generation(path, DateTime.Now));
        if (Generations.Count > MaxGenerations)
        {
            first = Generations.First!.Value.Path;
            Generations.RemoveFirst();
        }

        if (first is not null && Directory.Exists(first)) Directory.Delete(first, true);

        foreach (var file in files)
        {
            if (!IsValidName(file.Name))
                logger.LogWarning("File name contains invalid characters: {Name}", file.Name);
            else
            {
                var filePath = Path.Combine(path, file.Name);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    logger.LogWarning("File already exists, overwriting: {Name}", file.Name);
                }

                var dirName = Path.GetDirectoryName(filePath);
                if (dirName is null)
                {
                    logger.LogWarning("Failed to get directory name for {Name}", file.Name);
                    continue;
                }

                Directory.CreateDirectory(dirName);
                using var stream = File.Create(filePath);
                await file.CopyToAsync(stream);
            }
        }
    }

    public IEnumerable<string> Ls(int generation, ILogger<Backup> logger)
    {
        if (generation >= Generations.Count)
        {
            logger.LogWarning("Generation {Generation} does not exist", generation);
            return [];
        }
        var path = Generations.ElementAt(MaxGenerations - generation - 1).Path;
        var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        return files.Select(file => file[(path.Length + 1)..]);
    }

    public void Delete(string backupRoot)
    {
        var generationsPath = Path.Combine(backupRoot, Name);
        if (Directory.Exists(generationsPath)) Directory.Delete(generationsPath, true);
    }

    internal IResult GetFile(string file, int generation, ILogger<Backup> logger)
    {
        if (generation >= Generations.Count)
        {
            logger.LogWarning("Generation {Generation} does not exist", generation);
            return Results.NotFound();
        }
        if (!IsValidName(file))
        {
            logger.LogWarning("File name contains invalid characters: {Name}", file);
            return Results.BadRequest("File name contains invalid characters");
        }
        var filePath = Path.Combine(Generations.ElementAt(MaxGenerations - generation - 1).Path, file);
        if (!File.Exists(filePath))
        {
            logger.LogWarning("File {File} does not exist", file);
            return Results.NotFound();
        }
        return Results.File(filePath, "application/octet-stream", file);
    }
}
