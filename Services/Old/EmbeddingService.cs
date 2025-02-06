
namespace StartApi.Services;
    public class EmbeddingService2
    {
    
    PdfTextExtractorService pdfExtractorService = new PdfTextExtractorService();
    ProprocessTextService proprocessTextService = new ProprocessTextService();
     public async Task GeneratePdfEmbeddings() {
  // 1. Extract text from PDF
  string pdfPath = @"Services\Profile.pdf";
  string fullPath = Path.GetFullPath(pdfPath);
            Console.WriteLine($"PDF Path: {fullPath}");
  string text = pdfExtractorService.ExtractTextFromPdf(pdfPath);

  // 2. Preprocess text into chunks
  List<string> chunks = proprocessTextService.TextSplit(text);

  // 3. Initialize OpenAI client
  string apiKey = "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";
  var openAiClient = new OpenAiEmbeddingClient(apiKey);

  // 4. Generate embeddings for each chunk
  List<float[]> embeddings = new List<float[]>();
  foreach (var chunk in chunks) {
    float[] embedding = await openAiClient.GetEmbeddingAsync(chunk);
    embeddings.Add(embedding);
    Console.WriteLine($"Generated embedding for chunk of length {chunk.Length}");
    Console.WriteLine($"Embedding: {string.Join(", ", embedding)}");
    Console.WriteLine($"Chunk: {chunk}");
  }

  // Optional: Save embeddings to a database or file
}
    }