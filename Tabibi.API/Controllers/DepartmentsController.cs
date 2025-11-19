using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using Tabibi.API.Common;
using Tabibi.API.Common.Sorting;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Departments;
using Tabibi.API.Entities;
using Tabibi.API.Extensions;

namespace Tabibi.API.Controllers
{
    [Route("departments")]
    [ApiController]
    public sealed class DepartmentsController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] DepartmentQueryParameters query,
            SortMappingProvider sortMappingProvider,
            DataShapingProvider dataShapingProvider)
        {
            if (!sortMappingProvider.ValidateMappings<DepartmentDto, Department>(query.Sort))
            {
                return Problem(
                    detail: $"The provided sort parameter is not valid: '{query.Sort}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!dataShapingProvider.Validate<DepartmentDto>(query.Fields))
            {
                return Problem(
                    detail: $"The provided data shaping fields are not valid: '{query.Fields}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            SortMapping[] sortMappings = sortMappingProvider.GetMappings<DepartmentDto, Department>();

            query.Search ??= query.Search?.Trim().ToLower();

            IQueryable<DepartmentDto> departmentsQuery = dbContext.Departments
                .Where(d => query.Search == null ||
                            d.Name.ToLower().Contains(query.Search) ||
                            (d.Description != null && d.Description.ToLower().Contains(query.Search)))
                .ApplySort(query.Sort, sortMappings)
                .Select(d => d.ToDto());

            int totalCount = await departmentsQuery.CountAsync();

            List<DepartmentDto> departments = await departmentsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            PaginationResult<ExpandoObject> result = new()
            {
                Items = dataShapingProvider.ShapeCollectionData(departments, query.Fields),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            string id,
            string? fields,
            DataShapingProvider dataShapingProvider)
        {
            if (!dataShapingProvider.Validate<DepartmentDto>(fields))
            {
                return Problem(
                    detail: $"The provided data shaping fields are not valid: '{fields}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            Department? department = await dbContext.Departments.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id.ToString() == id);

            if (department == null)
            {
                return NotFound();
            }

            DepartmentDto departmentDto = department.ToDto();

            ExpandoObject shapedDepartmentDto = dataShapingProvider.ShapeData(departmentDto, fields);

            return Ok(shapedDepartmentDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateDepartmentDto createDepartmentDto,
            IValidator<CreateDepartmentDto> validator)
        {
            await validator.ValidateAndThrowAsync(createDepartmentDto);

            Department department = createDepartmentDto.ToEntity();

            await dbContext.Departments.AddAsync(department);
            await dbContext.SaveChangesAsync();

            DepartmentDto departmentDto = department.ToDto();

            return CreatedAtAction(nameof(GetById), new { id = departmentDto.Id }, departmentDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            string id,
            UpdateDepartmentDto updateDepartmentDto,
            IValidator<UpdateDepartmentDto> validator)
        {
            await validator.ValidateAndThrowAsync(updateDepartmentDto);

            Department? department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id.ToString() == id);

            if (department == null)
            {
                return NotFound();
            }

            department.UpdateFromDto(updateDepartmentDto);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            Department? department = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id.ToString() == id);

            if (department == null)
            {
                return NotFound();
            }

            dbContext.Departments.Remove(department);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
