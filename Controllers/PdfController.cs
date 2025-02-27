using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StartApi.Services;


namespace ragproject.Controllers
{
    [Route("[controller]")]
    public class PdfController : Controller
    {
    private readonly PdfTextExtractorService _pdfTextExtractorService;

    public PdfController(PdfTextExtractorService pdfTextExtractorService)
    {
        _pdfTextExtractorService = pdfTextExtractorService;
    }

        [HttpGet("GetPdf")]
        public IActionResult GetPdf()
        {
            return Ok("Pdf data");
        }

        [HttpPost("PostPdf")]
        public async Task<IActionResult> IngestPdf(IFormFile file)
        {
            try
            {
                var extractedText = _pdfTextExtractorService.ExtractTextFromPdf(file);
                return Ok(new { message = "Texto extraído com sucesso!", extractedText });
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

    }
}