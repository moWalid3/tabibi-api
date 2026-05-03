namespace Tabibi.API.Entities
{
    public sealed class MedicalProfile
    {
        public string PatientId { get; set; } = string.Empty;
        public Patient? Patient { get; set; }

        public List<string> ChronicDiseases { get; set; } = [];
        public List<string> Surgeries { get; set; } = [];

        public string? Medications { get; set; }
        public string? Allergies { get; set; }

        public double? Weight { get; set; }
        public double? Height { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
