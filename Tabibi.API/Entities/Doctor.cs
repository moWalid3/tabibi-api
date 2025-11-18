namespace Tabibi.API.Entities
{
    public sealed class Doctor : ApplicationUser
    {
        public bool IsApproved { get; set; }
        public string? Bio { get; set; }
        public decimal ConsultationFee { get; set; }
        public string? CredentialImageUrl { get; set; }
        public int YearsOfExperience { get; set; }

        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public Clinic? Clinic { get; set; }
    }
}
