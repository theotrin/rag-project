namespace StartApi.Services;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using StartApi.Models;

public class EmbeddingService
{
    private readonly HttpClient _httpClient;

    public EmbeddingService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("EmbeddingClient") ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var requestBody = new
        {
            model = "nomic-embed-text",
            input = text
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/embed", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Erro ao gerar embedding: {error}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Resposta da API de embeddings: {responseBody}");

        try
        {
            // Classe temporária para deserializar a resposta real da API
            var apiResponse = JsonSerializer.Deserialize<ApiEmbeddingResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse == null || apiResponse.Embeddings == null || apiResponse.Embeddings.Length == 0 || apiResponse.Embeddings[0] == null)
            {
                throw new Exception("Resposta da API de embeddings é inválida ou vazia.");
            }

            return apiResponse.Embeddings[0]; // Retorna o primeiro array de embeddings
        }
        catch (JsonException ex)
        {
            throw new Exception($"Erro ao deserializar a resposta da API de embeddings: {ex.Message}. Resposta: {responseBody}");
        }
    }

    // Classe temporária para corresponder à resposta da API
    private class ApiEmbeddingResponse
    {
        public string Model { get; set; }
        public float[][] Embeddings { get; set; }
    }
}