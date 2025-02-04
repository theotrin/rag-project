namespace StartApi.Services;

using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
    public class PdfTextExtractorService
    {
public string ExtractTextFromPdf(string pdfPath) {
  using (PdfReader reader = new PdfReader(pdfPath)) {
    StringBuilder text = new StringBuilder();
    for (int i = 1; i <= reader.NumberOfPages; i++) {
      text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
    }
    return text.ToString();
  }
}
    }