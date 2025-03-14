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

        // Normaliza o texto: substitui quebras de linha e múltiplos espaços por um espaço
        text = Regex.Replace(text, @"\s+", " ").Trim();

        // Corrige hífens perdidos apenas em casos específicos (ex.: "liga-la" -> "ligá-la")
        text = Regex.Replace(text, @"(\w)-(\w)", "$1-$2");

        // Divide em parágrafos ou seções numeradas (ex.: "01.", "a.")
        var sections = Regex.Split(text, @"(?<=[\d]+\.\s+|(?:[a-z]\.\s+))")
                           .Where(s => !string.IsNullOrWhiteSpace(s))
                           .ToList();

        string currentChunk = "";

        foreach (var section in sections)
        {
            string sectionTrimmed = section.Trim();
            if ((currentChunk.Length + sectionTrimmed.Length) <= maxChunkSize && !string.IsNullOrWhiteSpace(currentChunk))
            {
                currentChunk += " " + sectionTrimmed;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(currentChunk))
                {
                    string labeledChunk = label != null ? $"{currentChunk.Trim()} [Label: {label}]" : currentChunk.Trim();
                    float[] embedding = await _embeddingService.GenerateEmbeddingAsync(labeledChunk);
                    chunksWithEmbeddings.Add((labeledChunk, embedding));
                    Console.WriteLine($"Chunk gerado: {labeledChunk}");
                }
                currentChunk = sectionTrimmed;
            }
        }

        // Processa o último chunk
        if (!string.IsNullOrWhiteSpace(currentChunk))
        {
            string labeledChunk = label != null ? $"{currentChunk.Trim()} [Label: {label}]" : currentChunk.Trim();
            float[] embedding = await _embeddingService.GenerateEmbeddingAsync(labeledChunk);
            chunksWithEmbeddings.Add((labeledChunk, embedding));
            Console.WriteLine($"Chunk gerado: {labeledChunk}");
        }

        return chunksWithEmbeddings;
    }
}