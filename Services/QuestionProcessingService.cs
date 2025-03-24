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
        private readonly string _ollamaEmbeddingUrl = "http://localhost:11434/api/embeddings";
        private readonly string _ollamaChatUrl = "http://localhost:11434/api/chat";
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

            // Gera embedding da pergunta
            var questionEmbedding = await GenerateQuestionEmbeddingAsync(question);
            // Busca chunks similares no SurrealDB
            var similarChunks = await FetchSimilarChunksAsync(questionEmbedding, productLabel);
            // Mescla os chunks em um contexto
            string context = MergeChunks(similarChunks);

            // Monta o prompt para o Ollama
            string prompt = $@"
Como assistente de IA, sua tarefa é responder à seguinte pergunta com base no contexto fornecido. Sua resposta deve ser clara, concisa e diretamente relacionada à pergunta. Use apenas as informações presentes no contexto. Se o contexto não fornecer informações suficientes para responder com confiança, indique que a informação é insuficiente e não especule.

Contexto: {context}

Pergunta: {question}
";

            // Envia o prompt para o Ollama e obtém a resposta
            string llmResponse = await GetChatResponseAsync(prompt);
            return llmResponse;
        }

        // Método para gerar embedding da pergunta (já existente, mantido como está)
        private async Task<List<float>> GenerateQuestionEmbeddingAsync(string question)
        {
            var requestBody = new OllamaEmbeddingRequest
            {
                Model = "nomic-embed-text",
                Prompt = question
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _ollamaClient.PostAsync(_ollamaEmbeddingUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao gerar embedding da pergunta: {error}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var embeddingResponse = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (embeddingResponse?.Embedding == null || !embeddingResponse.Embedding.Any())
            {
                throw new Exception("Embedding não encontrado na resposta.");
            }

            return embeddingResponse.Embedding;
        }

        // Método para buscar chunks similares (já existente, mantido como está)
        private async Task<List<Chunk>> FetchSimilarChunksAsync(List<float> questionEmbedding, string productLabel)
        {
            string embeddingString = $"[{string.Join(", ", questionEmbedding.Select(e => $"{e}f"))}]";
            string sqlQuery = $@"
                LET $question = {embeddingString};
                SELECT content, vector::similarity::cosine(embedding, $question) AS dist 
                FROM test 
                WHERE label = '{productLabel}' AND embedding <|3|> $question 
                ORDER BY dist DESC 
                LIMIT 5;";

            var content = new StringContent(sqlQuery, Encoding.UTF8, "application/json");
            var response = await _surrealDbClient.PostAsync(_surrealDbUrl, content);
            

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao buscar chunks: {error}");
            }
            

            var responseBody = await response.Content.ReadAsStringAsync();
            var surrealResponse = JsonSerializer.Deserialize<List<SurrealDbResponse>>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (surrealResponse == null || surrealResponse.Count < 2 || surrealResponse[1].Status != "OK" || surrealResponse[1].Result == null)
            {
                return new List<Chunk>();
            }


var chunks = surrealResponse[1].Result.Select(r => new Chunk
{
    Content = r.TryGetValue("content", out var content) ? content.ToString() : "",
    Distance = r.TryGetValue("dist", out var dist) ? dist.GetDouble() : 0.0
}).ToList();

Console.WriteLine("Chunks encontrados:");
foreach (var chunk in chunks)
{
    Console.WriteLine($"Content: {chunk.Content}, Distance: {chunk.Distance}");
}

return chunks;
            
            
            
        }

        // Método para mesclar os chunks (já existente, mantido como está)
        private string MergeChunks(List<Chunk> chunks)
        {
            if (chunks == null || !chunks.Any())
                return "Nenhum contexto encontrado.";
            return string.Join(" ", chunks.Select(c => c.Content));
        }

        // Novo método para enviar o prompt ao Ollama e obter a resposta
        private async Task<string> GetChatResponseAsync(string prompt)
        {
            var requestBody = new
            {
                model = "llama3.2:1b",
                stream = false,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _ollamaClient.PostAsync(_ollamaChatUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao obter resposta do Ollama: {error}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonSerializer.Deserialize<OllamaChatResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (chatResponse?.Message?.Content == null)
            {
                throw new Exception("Resposta do Ollama não encontrada.");
            }

            return chatResponse.Message.Content;
        }
    }

    // Classe auxiliar para a resposta do Ollama Chat
    public class OllamaChatResponse
    {
        public string Model { get; set; }
        public string CreatedAt { get; set; }
        public Message Message { get; set; }
        public string DoneReason { get; set; }
        public bool Done { get; set; }
    }

    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    // Classes auxiliares já existentes (mantidas)
    public class Chunk
    {
        public string Content { get; set; }
        public double Distance { get; set; }
    }

    public class SurrealDbResponse
    {
        public string Status { get; set; }
        public List<Dictionary<string, JsonElement>> Result { get; set; }
        public string Time { get; set; }
    }
}