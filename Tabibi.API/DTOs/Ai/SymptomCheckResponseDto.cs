namespace Tabibi.API.DTOs.Ai
{
    public sealed record SymptomCheckResponseDto(
        List<string> Symptoms,
        List<string> PossibleConditions,
        string Urgency, // "low", "medium", "high"
        string RecommendedSpecialty,
        string Advice
    );
}
