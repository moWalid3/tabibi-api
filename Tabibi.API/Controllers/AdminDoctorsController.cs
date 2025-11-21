using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using Tabibi.API.Common;
using Tabibi.API.Common.Sorting;
using Tabibi.API.Database;
using Tabibi.API.DTOs.AdminDoctors;
using Tabibi.API.DTOs.AdminPatients;
using Tabibi.API.Entities;
using Tabibi.API.Extensions;

namespace Tabibi.API.Controllers
{
    //[Authorize(Roles = Roles.Admin)]
    [Route("admin/doctors")]
    [ApiController]
    public sealed class AdminDoctorsController(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<List<AdminDoctorDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllDoctors(
            AdminDoctorsQueryParameters query,
            SortMappingProvider sortMappingProvider,
            DataShapingProvider dataShapingProvider,
            IValidator<AdminDoctorsQueryParameters> validator)
        {
            await validator.ValidateAndThrowAsync(query);

            if (!sortMappingProvider.ValidateMappings<AdminDoctorDto, Doctor>(query.Sort))
            {
                return Problem(
                    detail: $"The provided sort parameter is not valid: '{query.Sort}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!dataShapingProvider.Validate<AdminDoctorDto>(query.Fields))
            {
                return Problem(
                    detail: $"The provided data shaping fields are not valid: '{query.Fields}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (query.CityId != null)
            {
                bool cityExists = await dbContext.Cities.AnyAsync(c => c.Id.ToString() == query.CityId);

                if (!cityExists)
                {
                    return Problem("Invalid cityId parameter",
                        statusCode: StatusCodes.Status400BadRequest);
                }
            }

            if (query.DepartmentId != null)
            {
                bool departmentExists = await dbContext.Departments
                    .AnyAsync(d => d.Id.ToString() == query.DepartmentId);

                if (!departmentExists)
                {
                    return Problem("Invalid departmentId parameter",
                        statusCode: StatusCodes.Status400BadRequest);
                }
            }

            query.Search ??= query.Search?.Trim().ToLower();

            IQueryable<AdminDoctorDto> patientsQuery = dbContext.Users.AsNoTracking()
                .OfType<Doctor>()
                .Include(p => p.Department)
                .Include(p => p.Clinic)
                .Include(p => p.Clinic!.City)
                .Where(p => query.Search == null ||
                            p.Name.ToLower().Contains(query.Search) ||
                            p.Email!.ToLower().Contains(query.Search) ||
                            p.Bio!.ToLower().Contains(query.Search))
                .Where(p => query.EmailConfirmed == null || p.EmailConfirmed == query.EmailConfirmed)
                .Where(p => query.Gender == null || p.Gender == query.Gender)
                .Where(p => query.Status == null || p.Status == query.Status)
                .Where(p => query.CityId == null || p.Clinic!.CityId.ToString() == query.CityId)
                .Where(p => query.DepartmentId == null || p.DepartmentId.ToString() == query.DepartmentId)
                .ApplySort(query.Sort, sortMappingProvider.GetMappings<AdminDoctorDto, Doctor>())
                .Select(p => p.ToDto());

            int totalCount = await patientsQuery.CountAsync();

            List<AdminDoctorDto> patients = await patientsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            PaginationResult<ExpandoObject> result = new()
            {
                Items = dataShapingProvider.ShapeCollectionData(patients, query.Fields),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            return Ok(result);
        }

    }
}
