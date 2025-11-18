namespace Tabibi.API.DTOs.Departments
{
    public sealed record DepartmentDto
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }
}
