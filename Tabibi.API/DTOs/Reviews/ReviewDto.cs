namespace Tabibi.API.DTOs.Reviews
{
    public sealed record ReviewDto
    {
        public Guid Id { get; init; }
        public int Rating { get; init; }
        public string? Comment { get; init; }
        public DateTime CreatedAt { get; init; }
        public required string PatientName { get; init; }
        public string? PatientAvatar { get; init; }
    }
}
