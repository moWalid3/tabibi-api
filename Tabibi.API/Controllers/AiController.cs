using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Ai;
using Tabibi.API.Entities;
using Tabibi.API.Services;

namespace Tabibi.API.Controllers
{
    [Authorize(Roles = Roles.Patient)]
    [Route("ai")]
    [ApiController]
    public sealed class AiController(
        AppDbContext dbContext,
        IGeminiAiService aiService) : ControllerBase
    {
        [HttpPost("symptom-check")]
        [ProducesResponseType<SymptomCheckResponseDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckSymptoms(
            SymptomCheckRequestDto request,
            IValidator<SymptomCheckRequestDto> validator)
        {
            try
            {
                await validator.ValidateAndThrowAsync(request);

                string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // 1. Fetch Patient's Medical Profile
                MedicalProfile? profile = await dbContext.MedicalProfiles
                    .FirstOrDefaultAsync(m => m.PatientId == patientId);

                string historyText = profile == null ? "No medical history recorded." :
                    $"- Chronic Diseases: {string.Join(", ", profile.ChronicDiseases)}\n" +
                    $"- Medications: {profile.Medications}\n" +
                    $"- Allergies: {profile.Allergies}";

                // 2. Build the Strict Prompt
                // We give Gemini very strict instructions so it acts like a medical tool, not a chatbot.
                string prompt = $@"
You are a highly intelligent medical symptom analyzer. 
Analyze the following user symptoms alongside their medical history.

User symptoms: '{request.SymptomsText}'

Medical history:
{historyText}

Tasks:
1. Extract the main symptoms.
2. Suggest possible conditions (Add a disclaimer that this is NOT a diagnosis).
3. Determine the urgency: 'low', 'medium', or 'high'.
4. Recommend the best medical specialty for these symptoms.
5. Provide short advice based on the symptoms and history.

Output Requirements:
Return ONLY a raw JSON object exactly matching this schema. Do not include markdown formatting like ```json.
{{
  ""symptoms"": [""string""],
  ""possibleConditions"": [""string""],
  ""urgency"": ""string"",
  ""recommendedSpecialty"": ""string"",
  ""advice"": ""string""
}}";

                // 3. Call the Gemini Service
                string? aiResponseJson = await aiService.AnalyzeSymptomsAsync(prompt);

                if (string.IsNullOrEmpty(aiResponseJson))
                {
                    return StatusCode(500, "Failed to analyze symptoms. Please try again later.");
                }


                // 4. Convert the JSON string from Gemini directly into our C# DTO
                SymptomCheckResponseDto? result = JsonSerializer.Deserialize<SymptomCheckResponseDto>(
                    aiResponseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "A crash occurred during AI processing!",
                    error = ex.Message
                });
            }
        }
    }
}
