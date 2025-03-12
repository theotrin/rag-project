using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace StartApi.Services;

public class ProprocessTextService
{
    private readonly HttpClient _httpClient;

    public ProprocessTextService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("EmbeddingClient") ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<List<(string Text, float[] Embedding)>> TextSplitAsync(string text, string? label = null, int maxChunkSize = 500)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("O texto não pode ser vazio.");

        List<(string Text, float[] Embedding)> chunksWithEmbeddings = new();

        for (int i = 0; i < text.Length; i += maxChunkSize)
        {
            int length = Math.Min(maxChunkSize, text.Length - i);
            string chunk = text.Substring(i, length).Trim();
            string labeledChunk = label != null ? $"{chunk} [Label: {label}]" : chunk;

            float[] embedding = await GenerateEmbeddingAsync(labeledChunk);
            chunksWithEmbeddings.Add((labeledChunk, embedding));
            Console.WriteLine($"Chunk gerado: {labeledChunk}");
        }

        return chunksWithEmbeddings;
    }

    private async Task<float[]> GenerateEmbeddingAsync(string text)
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
            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Tolerar diferenças de maiúsculas/minúsculas
            });

            if (embeddingResponse == null || embeddingResponse.Embeddings == null || embeddingResponse.Embeddings.Length == 0)
            {
                throw new Exception("Resposta da API de embeddings é inválida ou vazia.");
            }

            return embeddingResponse.Embeddings[0];
        }
        catch (JsonException ex)
        {
            throw new Exception($"Erro ao deserializar a resposta da API de embeddings: {ex.Message}. Resposta: {responseBody}");
        }
    }
}

public class EmbeddingResponse
{
    public string Model { get; set; }
    public float[][] Embeddings { get; set; }
}