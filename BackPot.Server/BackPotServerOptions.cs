namespace BackPot.Server;
internal class BackPotServerOptions
{
    public int Port { get; set; } = 8080;
    public string BackupRoot { get; set; } = "/data";
    public string Token { get; set; } = Guid.NewGuid().ToString();
    public int MaxGenerations { get; set; } = 3;
}