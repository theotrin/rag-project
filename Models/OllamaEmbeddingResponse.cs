using System.Text.Json.Serialization;

public class OllamaEmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public List<float> Embedding { get; set; } = new List<float>();
}