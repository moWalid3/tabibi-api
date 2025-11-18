using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Departments;
using Tabibi.API.Entities;

namespace Tabibi.API.Controllers
{
    [Route("departments")]
    [ApiController]
    public sealed class DepartmentsController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DepartmentQueryParameters query)
        {
            List<DepartmentDto> departments = await dbContext.Departments
                .Where(d => query.Search == null ||
                            d.Name.ToLower().Contains(query.Search) ||
                            (d.Description != null && d.Description.ToLower().Contains(query.Search)))
                .Select(d => d.ToDto())
                .ToListAsync();

            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            Department? department = await dbContext.Departments.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id.ToString() == id);

            if (department == null)
            {
                return NotFound();
            }

            DepartmentDto departmentDto = department.ToDto();

            return Ok(departmentDto);
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
