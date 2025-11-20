using Tabibi.API.Entities.Enums;

namespace Tabibi.API.Entities
{
    public sealed class Doctor : ApplicationUser
    {
        public DoctorStatus Status { get; set; }
        public string? Bio { get; set; }
        public decimal ConsultationFee { get; set; }
        public string? CredentialImageUrl { get; set; }
        public int YearsOfExperience { get; set; }

        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public Clinic? Clinic { get; set; }
    }
}
