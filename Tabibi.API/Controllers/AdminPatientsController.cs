using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using Tabibi.API.Common;
using Tabibi.API.Common.Sorting;
using Tabibi.API.Database;
using Tabibi.API.DTOs.AdminPatients;
using Tabibi.API.DTOs.Departments;
using Tabibi.API.Entities;
using Tabibi.API.Extensions;

namespace Tabibi.API.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Route("admin/patients")]
    [ApiController]
    public sealed class AdminPatientsController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<List<PatientDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllPatients(
            AdminPatientsQueryParameters query,
            SortMappingProvider sortMappingProvider,
            DataShapingProvider dataShapingProvider,
            IValidator<AdminPatientsQueryParameters> validator)
        {
            await validator.ValidateAndThrowAsync(query);

            if (!sortMappingProvider.ValidateMappings<PatientDto, Patient>(query.Sort))
            {
                return Problem(
                    detail: $"The provided sort parameter is not valid: '{query.Sort}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!dataShapingProvider.Validate<PatientDto>(query.Fields))
            {
                return Problem(
                    detail: $"The provided data shaping fields are not valid: '{query.Fields}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            query.Search ??= query.Search?.Trim().ToLower();


            IQueryable<PatientDto> patientsQuery = dbContext.Users.AsNoTracking()
                .OfType<Patient>()
                .Include(p => p.City)
                .Where(p => query.Search == null ||
                            p.Name.ToLower().Contains(query.Search) ||
                            p.Email!.ToLower().Contains(query.Search))
                .Where(p => query.EmailConfirmed == null || p.EmailConfirmed == query.EmailConfirmed)
                .Where(p => query.Gender == null || p.Gender == query.Gender)
                .ApplySort(query.Sort, sortMappingProvider.GetMappings<PatientDto, Patient>())
                .Select(p => p.ToDto());

            int totalCount = await patientsQuery.CountAsync();

            List<PatientDto> patients = await patientsQuery
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
