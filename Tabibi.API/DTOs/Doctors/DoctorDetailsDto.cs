using Tabibi.API.DTOs.Reviews;
using Tabibi.API.DTOs.WorkSchedule;

namespace Tabibi.API.DTOs.Doctors
{
    public sealed record DoctorDetailsDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public string? AvatarUrl { get; init; }
        public required string Department { get; init; }
        public required string Address { get; init; }
        public string? Bio { get; init; }
        public int YearsOfExperience { get; init; }
        public decimal ConsultationFee { get; init; }
        public double Rating { get; init; }
        public int ReviewCount { get; init; }
        public int PatientCount { get; init; }
        public bool IsFavorited { get; init; }
        public List<ReviewDto> Reviews { get; init; } = [];
        public List<WorkScheduleDto>? Schedule { get; init; }
    }
}
