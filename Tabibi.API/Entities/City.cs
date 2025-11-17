namespace Tabibi.API.Entities
{
    public sealed class City
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }

        public List<Clinic> Clinics { get; } = [];
        public List<Patient> Patients { get; } = [];
    }
}
