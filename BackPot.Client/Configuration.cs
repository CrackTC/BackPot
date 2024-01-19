namespace BackPot.Client;

internal class Configuration(string host, string token, string cron, string name, string root)
{
    public string Host { get; init; } = host;
    public string Token { get; init; } = token;
    public string Cron { get; init; } = cron;
    public string Name { get; init; } = name;
    public string Root { get; init; } = root;

    public static Configuration GetConfiguration() => new(
        Environment.GetEnvironmentVariable("HOST") is string host ? host : "localhost",
        Environment.GetEnvironmentVariable("TOKEN") is string token ? token : "",
        Environment.GetEnvironmentVariable("CRON") is string cron ? cron : "0 0 0 * * ?",
        Environment.GetEnvironmentVariable("NAME") is string name ? name : throw new ArgumentNullException("NAME"),
        Environment.GetEnvironmentVariable("ROOT") is string root ? root : "/data"
    );
}
