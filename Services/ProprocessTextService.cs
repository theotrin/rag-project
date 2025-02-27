
namespace StartApi.Services;
    public class ProprocessTextService
    {
        public List<string> TextSplit(string text, string? label = null, int maxChunkSize = 500)
    {
        List<string> chunks = new List<string>();
        for (int i = 0; i < text.Length; i += maxChunkSize)
        {
            int length = Math.Min(maxChunkSize, text.Length - i);
            string chunk = text.Substring(i, length).Trim();
            // Adiciona a label apenas se ela for fornecida
            chunks.Add(label != null ? $"{chunk} [Label: {label}]" : chunk);
        }
        foreach(var chunkString in chunks)
        {
            Console.WriteLine(chunkString);
        }
        return chunks;
    }
    }
