namespace StartApi.Services;
using System.Net.Http.Json;
using System.Text.Json;
using StartApi.Models;

public class EmbeddingService
{
    public async Task<List<float>> GenerateEmbeddingAsync(string prompt)
    {
        using var httpClient = new HttpClient();
        var request = new OllamaEmbeddingRequest
        {
            Model = "nomic-embed-text",
            Prompt = prompt
        };

        var response = await httpClient.PostAsJsonAsync(
            "http://localhost:11434/api/embeddings",
            request
        );

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed with status code: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
        return responseContent?.Embedding ?? new List<float>();
    }
}
