using Stripe;
using System.Text;
using System.Text.Json;

namespace Tabibi.API.Services
{
    public class GeminiAiService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<GeminiAiService> logger) : IGeminiAiService
    {
        private readonly string _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini ApiKey is missing in appsettings.json");

        public async Task<string?> AnalyzeSymptomsAsync(string promptText)
        {
            // We use gemini-2.5-flash because it is the fastest and free tier model
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            // This is the specific JSON structure Google's API expects
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = promptText } } }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json" // CRITICAL: This forces Gemini to return strict JSON, not chat text!
                }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            StringContent content = new(jsonBody, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                // If Google rejects it, we read THEIR exact error message and throw it!
                string googleError = await response.Content.ReadAsStringAsync();
                logger.LogError($"Gemini API Error: {googleError}");
                throw new Exception($"Google API Error ({response.StatusCode}): {googleError}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);

            string generatedText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString() ?? "";

            return generatedText;
        }
    }
}
