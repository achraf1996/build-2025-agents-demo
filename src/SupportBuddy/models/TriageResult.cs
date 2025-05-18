using System.Collections.Generic;
using System.Text.Json.Serialization;

public class TriageResult
{
    [JsonPropertyName("questions")]
    public List<string> Questions { get; set; } = new();

    [JsonPropertyName("answers")]
    public List<string> Issues { get; set; } = new();
}
