using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StartApi.Models;
using StartApi.Services;


namespace ragproject.Controllers
{
    [Route("[controller]")]
    public class PdfController : Controller
    {
    private readonly PdfTextExtractorService _pdfTextExtractorService;
    private readonly ProprocessTextService _proprocessTextService;
    private readonly QuestionProcessingService _questionProcessingService;

        public PdfController(PdfTextExtractorService pdfTextExtractorService, ProprocessTextService proprocessTextService, QuestionProcessingService questionProcessingService)
        {
            _pdfTextExtractorService = pdfTextExtractorService;
            _proprocessTextService = proprocessTextService;
            _questionProcessingService = questionProcessingService;
        }

        [HttpGet("GetPdf")]
        public IActionResult GetPdf()
        {
            return Ok("Pdf data");
        }

    [HttpPost("ingest-pdf")]
    public IActionResult IngestPdf(IFormFile file, [FromQuery] string? label = null)
    {
        try
        {
            // Passo 1: Extrair o texto do PDF
            var extractedText = _pdfTextExtractorService.ExtractTextFromPdf(file);

            // Passo 2: Dividir em chunks e adicionar a label (se fornecida)
            var chunks = _proprocessTextService.TextSplit(extractedText, label);

            return Ok(new
            {
                message = "PDF ingerido e dividido em chunks com sucesso!",
                chunksCount = chunks.Count,
                chunks
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao processar o PDF: {ex.Message}");
        }
    }

    [HttpPost("ask-question")]
    public IActionResult AskQuestion([FromBody] AskQuestionRequest request)
    {
        try
        {
            var response = _questionProcessingService.ProcessQuestion(request.Question, request.ProductLabel);
            return Ok(new
            {
                message = "Pergunta processada com sucesso!",
                question = request.Question,
                productLabel = request.ProductLabel,
                response
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
}