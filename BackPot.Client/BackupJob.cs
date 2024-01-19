using System.Net.Http.Headers;
using Quartz;

namespace BackPot.Client;

internal class BackupJob : IJob
{
    private static readonly HttpClient _client;

    static BackupJob()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("X-Token", Program.Configuration.Token);
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var content = new MultipartFormDataContent();

        string[] filePaths;
        try
        {
            filePaths = Directory.GetFiles(Program.Configuration.Root, "*", SearchOption.AllDirectories);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to get files: {ex.Message}");
            return;
        }

        foreach (var filePath in filePaths)
        {
            Console.WriteLine($"Backing up {filePath}");
            var relativePath = filePath.Replace(Program.Configuration.Root, "").Replace('\\', '/');
            var fileContent = new StreamContent(File.OpenRead(filePath));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, relativePath, Path.GetFileName(filePath));
        }

        var response = await _client.PostAsync($"{Program.Configuration.Host}/backups/{Program.Configuration.Name}", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Successfully backed up {filePaths.Length} files");
        }
        else
        {
            Console.Error.WriteLine($"Failed to back up files: [{response.StatusCode}] {responseContent}");
        }
    }
}