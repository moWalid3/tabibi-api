using Tabibi.API.Common.Sorting;
using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.Departments
{
    public static class DepartmentMappings
    {
        public static readonly SortMappingDefinition<DepartmentDto, Department> SortMapping = new()
        {
            Mappings = [
                new SortMapping(nameof(DepartmentDto.Name), nameof(Department.Name)),
                new SortMapping(nameof(DepartmentDto.Description), nameof(Department.Description)),
                new SortMapping(nameof(DepartmentDto.CreatedAtUtc), nameof(Department.CreatedAtUtc))
            ]
        };

        public static DepartmentDto ToDto(this Department department)
        {
            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                ImageUrl = department.ImageUrl,
                CreatedAtUtc = department.CreatedAtUtc
            };
        }

        public static Department ToEntity(this CreateDepartmentDto dto)
        {
            return new Department
            {
                Id = Guid.CreateVersion7(),
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        public static void UpdateFromDto(this Department department, UpdateDepartmentDto dto)
        {
            department.Name = dto.Name;
            department.Description = dto.Description;
            department.ImageUrl = dto.ImageUrl;
        }
    }
}
