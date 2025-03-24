using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

public class PdfTextExtractorService
{
    public string ExtractTextFromPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Nenhum arquivo enviado.");

        if (!file.FileName.EndsWith(".pdf"))
            throw new ArgumentException("Apenas arquivos PDF são aceitos.");

        using (var stream = file.OpenReadStream())
        using (var reader = new PdfReader(stream))
        {
            StringBuilder text = new StringBuilder();
            HashSet<string> seenLines = new HashSet<string>(); // Para rastrear linhas únicas

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                string pageText = PdfTextExtractor.GetTextFromPage(reader, i);
                string[] lines = pageText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedLine) && !seenLines.Contains(trimmedLine))
                    {
                        seenLines.Add(trimmedLine);
                        text.AppendLine(trimmedLine); // Adiciona linha única
                    }
                }
            }
            Console.WriteLine("Texto extraído do PDF:");
            Console.WriteLine(text.ToString());
            return text.ToString();
        }
    }
}