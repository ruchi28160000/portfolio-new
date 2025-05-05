using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PortfolioTrackerApi.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly HttpClient _httpClient;

        // Constructor to inject HttpClient
        public TranslationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Implementing the TranslateTextAsync method from ITranslationService
        public async Task<string> TranslateTextAsync(string text, string sourceLang, string targetLang)
        {
            // Mymemory API URL format
            var url = $"https://api.mymemory.translated.net/get?q={System.Net.WebUtility.UrlEncode(text)}&langpair={sourceLang}|{targetLang}";

            try
            {
                // Sending the GET request to Mymemory API
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // Read the JSON response content
                    var content = await response.Content.ReadAsStringAsync();
                    return ExtractTranslatedText(content);
                }
                else
                {
                    // Return an error message if the request failed
                    return "Error: Unable to fetch translation.";
                }
            }
            catch (Exception ex)
            {
                // Return exception message in case of any error
                return $"Exception: {ex.Message}";
            }
        }

        // Helper method to extract translated text from JSON response
        private string ExtractTranslatedText(string responseJson)
        {
            try
            {
                // Deserialize the JSON response into a JsonElement
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

                // Check if 'responseData' and 'translatedText' properties exist in the JSON
                if (jsonResponse.TryGetProperty("responseData", out var responseData) &&
                    responseData.TryGetProperty("translatedText", out var translatedText))
                {
                    // Return the translated text
                    return translatedText.GetString();
                }

                // If the translation or responseData is missing
                return "Translation not found in the response.";
            }
            catch (JsonException)
            {
                // Handle any issues with deserialization
                return "Error: Unable to parse the translation response.";
            }
        }
    }
}
