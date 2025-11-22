namespace Tabibi.API.DTOs.Doctors
{
    public sealed record DoctorBasicDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public string? AvatarUrl { get; init; }
        public decimal ConsultationFee { get; init; }
        public int YearsOfExperience { get; init; }
        public string? Address { get; init; }
        public string? Department { get; init; }
    }
}
