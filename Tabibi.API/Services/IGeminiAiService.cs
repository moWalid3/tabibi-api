namespace Tabibi.API.Services
{
    public interface IGeminiAiService
    {
        Task<string?> AnalyzeSymptomsAsync(string promptText);
    }
}
