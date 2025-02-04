using System.Text.Json.Serialization;

namespace StartApi.Models;

    public class OllamaEmbeddingRequest
    {
    [JsonPropertyName("model")]
    public string Model { get; set; } = "nomic-embed-text";

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = "";
    }
