using System.Text.Json.Serialization;

namespace StartApi.Models
{
    public class OllamaEmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public List<float> Embedding { get; set; }
    }
}