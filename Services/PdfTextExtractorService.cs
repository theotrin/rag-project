namespace StartApi.Services;

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
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
            }
            //  Console.WriteLine(text.ToString());
            return text.ToString();
        }
    }
    }