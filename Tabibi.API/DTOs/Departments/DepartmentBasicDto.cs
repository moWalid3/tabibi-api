namespace Tabibi.API.DTOs.Departments
{
    public sealed record DepartmentBasicDto
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
    }
}
