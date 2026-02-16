using System.Text.Json.Serialization;

namespace DuudlagaFlow.Models;

public class TranscriptionRecord
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.Now;

    [JsonPropertyName("durationSeconds")]
    public double DurationSeconds { get; set; }
}
