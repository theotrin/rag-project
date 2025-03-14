// Services/QuestionProcessingService.cs
namespace StartApi.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using StartApi.Models;
    

    public class QuestionProcessingService
    {
        
        private readonly HttpClient _ollamaClient;
        private readonly HttpClient _surrealDbClient;
        private readonly string _ollamaUrl = "http://localhost:11434/api/embeddings";
        private readonly string _surrealDbUrl = "http://localhost:8001/sql";

        public QuestionProcessingService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _ollamaClient = httpClientFactory.CreateClient("OllamaClient") ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _surrealDbClient = httpClientFactory.CreateClient("SurrealDbClient") ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<string> ProcessQuestionAsync(string question, string productLabel)
        {
            if (string.IsNullOrEmpty(question))
                throw new ArgumentException("A pergunta não pode ser vazia.");
            if (string.IsNullOrEmpty(productLabel))
                throw new ArgumentException("A identificação do produto não pode ser vazia.");

            var questionEmbedding = await GenerateQuestionEmbeddingAsync(question);
            var similarChunks = await FetchSimilarChunksAsync(questionEmbedding, productLabel);
            string context = MergeChunks(similarChunks);

            return context;
        }

        private async Task<List<float>> GenerateQuestionEmbeddingAsync(string question)
        {
            var requestBody = new OllamaEmbeddingRequest
    {
        Model = "nomic-embed-text",
        Prompt = question
    };

    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
    var response = await _ollamaClient.PostAsync(_ollamaUrl, content);

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"Erro ao gerar embedding da pergunta: {error}");
    }

    var responseBody = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Resposta da API de embeddings: {responseBody}");

    var embeddingResponse = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(responseBody, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    Console.WriteLine($"Resultado da desserialização: embeddingResponse = {(embeddingResponse == null ? "null" : "não null")}, Embedding = {(embeddingResponse?.Embedding == null ? "null" : embeddingResponse.Embedding.Count + " elementos")}");

    if (embeddingResponse?.Embedding == null || !embeddingResponse.Embedding.Any())
    {
        throw new Exception("Embedding não encontrado na resposta.");
    }

    return embeddingResponse.Embedding; // Correção: retorna a lista diretamente
}

       private async Task<List<Chunk>> FetchSimilarChunksAsync(List<float> questionEmbedding, string productLabel)
{
    // Converte o embedding para string no formato correto
    string embeddingString = $"[{string.Join(", ", questionEmbedding.Select(e => $"{e}f"))}]";
    Console.WriteLine($"Embedding gerado: {embeddingString}");

    // Monta a consulta SQL
    string sqlQuery = $@"
        LET $question = {embeddingString};
        SELECT sample, content, vector::similarity::cosine(embedding, $question) AS dist FROM test WHERE embedding <|3|> $question;";
    Console.WriteLine($"Consulta SQL: {sqlQuery}");

    // Envia a requisição ao SurrealDB
    var content = new StringContent(sqlQuery, Encoding.UTF8, "application/json");
    var response = await _surrealDbClient.PostAsync(_surrealDbUrl, content);

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Erro no SurrealDB: {error}");
        throw new Exception($"Erro ao buscar chunks: {error}");
    }

    // Lê e loga a resposta crua
    var responseBody = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Resposta do SurrealDB: {responseBody}");

    // Desserializa a resposta
    var surrealResponse = JsonSerializer.Deserialize<List<SurrealDbResponse>>(responseBody, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true // Tolerar variações de maiúsculas/minúsculas
    });
    Console.WriteLine($"Resposta desserializada: {JsonSerializer.Serialize(surrealResponse)}");

    if (surrealResponse == null || surrealResponse.Count < 2)
    {
        Console.WriteLine("Resposta inválida ou sem resultados (count < 2).");
        return new List<Chunk>();
    }

    // Verifica o segundo elemento da resposta (índice 1)
    var resultElement = surrealResponse[1];
    if (resultElement.Status != "OK" || resultElement.Result == null || !resultElement.Result.Any())
    {
        Console.WriteLine($"Nenhum resultado válido encontrado. Status: {resultElement.Status}, Result: {resultElement.Result?.Count ?? 0}");
        return new List<Chunk>();
    }

    // Converte os resultados em objetos Chunk
    var chunks = resultElement.Result.Select(r => new Chunk
    {
        Content = r.TryGetValue("content", out var content) ? content.ToString() : "",
        Distance = r.TryGetValue("dist", out var dist) ? dist.GetDouble() : 0.0
    }).ToList();

    Console.WriteLine($"Chunks encontrados: {chunks.Count}");
    foreach (var chunk in chunks)
    {
        Console.WriteLine($"Chunk - Content: {chunk.Content}, Distance: {chunk.Distance}");
    }

    return chunks;
}

        private string MergeChunks(List<Chunk> chunks)
        {
            if (chunks == null || !chunks.Any())
                return "Nenhum contexto encontrado.";
            return string.Join(" ", chunks.Select(c => c.Content));
        }
    }

    // Classe auxiliar para os chunks
    public class Chunk
    {
        public string Content { get; set; }
        public double Distance { get; set; }
    }

    // Classe auxiliar para a resposta do SurrealDB
    public class SurrealDbResponse
{
    public string Status { get; set; }
    public List<Dictionary<string, JsonElement>> Result { get; set; }
    public string Time { get; set; }
}
}