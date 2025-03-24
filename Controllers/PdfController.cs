using Microsoft.AspNetCore.Mvc;
using StartApi.Models;
using StartApi.Services;
using System.Text;
using System.Text.Json;

namespace ragproject.Controllers;

[Route("[controller]")]
public class PdfController : Controller
{
    private readonly PdfTextExtractorService _pdfTextExtractorService;
    private readonly ProprocessTextService _proprocessTextService;
    private readonly QuestionProcessingService _questionProcessingService;
    private readonly HttpClient _surrealDbClient;

    public PdfController(
        PdfTextExtractorService pdfTextExtractorService,
        ProprocessTextService proprocessTextService,
        QuestionProcessingService questionProcessingService,
        IHttpClientFactory httpClientFactory)
    {
        _pdfTextExtractorService = pdfTextExtractorService ?? throw new ArgumentNullException(nameof(pdfTextExtractorService));
        _proprocessTextService = proprocessTextService ?? throw new ArgumentNullException(nameof(proprocessTextService));
        _questionProcessingService = questionProcessingService ?? throw new ArgumentNullException(nameof(questionProcessingService));
        _surrealDbClient = httpClientFactory.CreateClient("SurrealDbClient") ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    [HttpPost("ingest-pdf")]
    public async Task<IActionResult> IngestPdf(IFormFile file, [FromQuery] string? label = null)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("Nenhum arquivo enviado.");

            var extractedText = _pdfTextExtractorService.ExtractTextFromPdf(file);
            if (string.IsNullOrEmpty(extractedText))
                return BadRequest("Nenhum texto extraído do PDF.");

            var chunksWithEmbeddings = await _proprocessTextService.TextSplitAsync(extractedText, label);
            if (chunksWithEmbeddings == null || !chunksWithEmbeddings.Any())
                return BadRequest("Nenhum chunk gerado.");

            Console.WriteLine($"Iniciando inserção de {chunksWithEmbeddings.Count} chunks no SurrealDB...");
            foreach (var (text, embedding) in chunksWithEmbeddings)
            {
                string sqlQuery = $"INSERT INTO test {{ content: '{text.Replace("'", "\\'")}', embedding: {JsonSerializer.Serialize(embedding)}, label: '{label ?? "sem_label"}' }};";
                var content = new StringContent(sqlQuery, Encoding.UTF8, "application/json");
                Console.WriteLine($"Enviando query ao SurrealDB: {sqlQuery}");
                var response = await _surrealDbClient.PostAsync("sql", content);

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Resposta do SurrealDB: Status {response.StatusCode}, Corpo: {responseBody}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao inserir no SurrealDB: {responseBody}");
                }
            }

            return Ok(new
            {
                message = "PDF ingerido e chunks armazenados no SurrealDB com sucesso!",
                chunksCount = chunksWithEmbeddings.Count,
                chunks = chunksWithEmbeddings.Select(c => c.Text).ToList()
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro detalhado: {ex}");
            return StatusCode(500, new { error = "Erro ao processar o PDF", details = ex.Message });
        }
    }

   // Controllers/PdfController.cs
[HttpPost("ask-question")]
public async Task<IActionResult> AskQuestion([FromBody] AskQuestionRequest request)
{
    try
    {
        var answer = await _questionProcessingService.ProcessQuestionAsync(request.Question, request.ProductLabel);
        return Ok(new
        {
            message = "Pergunta processada com sucesso!",
            question = request.Question,
            productLabel = request.ProductLabel,
            answer
        });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "Erro ao processar a pergunta", details = ex.Message });
    }
}
}