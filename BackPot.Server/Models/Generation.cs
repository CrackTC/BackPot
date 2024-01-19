using System.Text.Json.Serialization;

namespace BackPot.Server.Models;

[Serializable]
public record Generation([property: JsonPropertyName("path")] string Path, [property: JsonPropertyName("date")] DateTime Date);
