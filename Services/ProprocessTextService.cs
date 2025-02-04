
namespace StartApi.Services;
    public class ProprocessTextService
    {
        public List<string> PreprocessText(string text, int maxChunkSize = 500) {
        // Split text into chunks (e.g., paragraphs or sentences)
        List<string> chunks = new List<string>();
        for (int i = 0; i < text.Length; i += maxChunkSize) {
            int length = Math.Min(maxChunkSize, text.Length - i);
            chunks.Add(text.Substring(i, length).Trim());
        }
        return chunks;
        }
    }
