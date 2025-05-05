using Microsoft.AspNetCore.Mvc;
using PortfolioTrackerApi.Services; // Make sure the namespace is correct
using System.Threading.Tasks;

namespace PortfolioTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslationController : ControllerBase
    {
        private readonly TranslationService _translationService;

        // Inject TranslationService through the constructor
        public TranslationController(TranslationService translationService)
        {
            _translationService = translationService;
        }

        // POST api/translation/translate
        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
        {
            // Validate incoming request
            if (string.IsNullOrEmpty(request.Text) || string.IsNullOrEmpty(request.SourceLang) || string.IsNullOrEmpty(request.TargetLang))
            {
                return BadRequest("Missing required parameters.");
            }

            // Get translation result from TranslationService
            var translatedText = await _translationService.TranslateTextAsync(request.Text, request.SourceLang, request.TargetLang);

            // Return translated text as a JSON response
            if (!string.IsNullOrEmpty(translatedText))
            {
                return Ok(new { TranslatedText = translatedText });
            }
            else
            {
                return StatusCode(500, "Translation failed.");
            }
        }
    }

    // DTO to hold translation request data
    public class TranslationRequest
    {
        public string Text { get; set; }
        public string SourceLang { get; set; }
        public string TargetLang { get; set; }
    }
}
