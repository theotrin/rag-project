namespace StartApi.Services;

using System.Text.RegularExpressions;

public class ProprocessTextService
{
    private readonly EmbeddingService _embeddingService;

    public ProprocessTextService(EmbeddingService embeddingService)
    {
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
    }

    public async Task<List<(string Text, float[] Embedding)>> TextSplitAsync(string text, string? label = null, int maxChunkSize = 500)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("O texto não pode ser vazio.");

        List<(string Text, float[] Embedding)> chunksWithEmbeddings = new();
        HashSet<string> seenChunks = new HashSet<string>(); // Para evitar duplicatas

        // Normaliza o texto: substitui quebras de linha e múltiplos espaços por um espaço
        text = Regex.Replace(text, @"\s+", " ").Trim();

        // Corrige hífens perdidos
        text = Regex.Replace(text, @"(\w)-(\w)", "$1-$2");

        // Divide em sentenças ou seções usando ponto final ou marcadores
        var sections = Regex.Split(text, @"(?<=[.!?])\s+|\s*•\s*")
                           .Where(s => !string.IsNullOrWhiteSpace(s))
                           .Select(s => s.Trim())
                           .ToList();

        string currentChunk = "";

        foreach (var section in sections)
        {
            if ((currentChunk.Length + section.Length) <= maxChunkSize && !string.IsNullOrWhiteSpace(currentChunk))
            {
                currentChunk += " " + section;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(currentChunk))
                {
                    string labeledChunk = label != null ? $"{currentChunk.Trim()} [Label: {label}]" : currentChunk.Trim();
                    if (seenChunks.Add(labeledChunk)) // Só adiciona se for único
                    {
                        float[] embedding = await _embeddingService.GenerateEmbeddingAsync(labeledChunk);
                        chunksWithEmbeddings.Add((labeledChunk, embedding));
                        Console.WriteLine($"Chunk gerado: {labeledChunk}");
                    }
                }
                currentChunk = section;
            }
        }

        // Processa o último chunk
        if (!string.IsNullOrWhiteSpace(currentChunk))
        {
            string labeledChunk = label != null ? $"{currentChunk.Trim()} [Label: {label}]" : currentChunk.Trim();
            if (seenChunks.Add(labeledChunk)) // Só adiciona se for único
            {
                float[] embedding = await _embeddingService.GenerateEmbeddingAsync(labeledChunk);
                chunksWithEmbeddings.Add((labeledChunk, embedding));
                Console.WriteLine($"Chunk gerado: {labeledChunk}");
            }
        }

        return chunksWithEmbeddings;
    }
}