using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Security.Claims;
using System.Transactions;
using Tabibi.API.Common;
using Tabibi.API.Common.Sorting;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Doctors;
using Tabibi.API.DTOs.Reviews;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;
using Tabibi.API.Extensions;

namespace Tabibi.API.Controllers
{
    [Route("doctors")]
    [ApiController]
    public sealed class DoctorsController(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext) : ControllerBase
    {
        [Authorize(Roles = Roles.Patient)]
        [HttpGet]
        [ProducesResponseType<List<DoctorBasicDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllDoctors(
            DoctorsQueryParameters query,
            SortMappingProvider sortMappingProvider,
            DataShapingProvider dataShapingProvider,
            IValidator<DoctorsQueryParameters> validator)
        {
            await validator.ValidateAndThrowAsync(query);

            if (!sortMappingProvider.ValidateMappings<DoctorBasicDto, Doctor>(query.Sort))
            {
                return Problem(
                    detail: $"The provided sort parameter is not valid: '{query.Sort}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!dataShapingProvider.Validate<DoctorBasicDto>(query.Fields))
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

            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            query.Search ??= query.Search?.Trim().ToLower();

            IQueryable<DoctorBasicDto> patientsQuery = dbContext.Users.AsNoTracking()
                .OfType<Doctor>()
                .Include(d => d.Department)
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.City)
                .Include(d => d.Favorites)
                .Where(d => d.Status == DoctorStatus.Approved)
                .Where(d => query.Search == null ||
                            d.Name.ToLower().Contains(query.Search) ||
                            d.Email!.ToLower().Contains(query.Search) ||
                            d.Bio!.ToLower().Contains(query.Search))
                .Where(d => query.Gender == null || d.Gender == query.Gender)
                .Where(d => query.CityId == null || (d.Clinic != null && d.Clinic.CityId.ToString() == query.CityId))
                .Where(d => query.DepartmentId == null || d.DepartmentId.ToString() == query.DepartmentId)
                .ApplySort(query.Sort, sortMappingProvider.GetMappings<DoctorBasicDto, Doctor>())
                .Select(d => d.ToDoctorBasicDto(patientId));

            int totalCount = await patientsQuery.CountAsync();

            List<DoctorBasicDto> patients = await patientsQuery
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


        [Authorize(Roles = Roles.Patient)]
        [HttpGet("map")]
        [ProducesResponseType<List<DoctorMapPinDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDoctorsOnMap(
            double minLat,
            double maxLat,
            double minLng,
            double maxLng,
            string? departmentId)
        {
            if (minLat >= maxLat || minLng >= maxLng)
            {
                return BadRequest("Invalid map coordinates. Min must be less than Max.");
            }

            if (departmentId != null)
            {
                bool departmentExists = await dbContext.Departments
                    .AnyAsync(d => d.Id.ToString() == departmentId);

                if (!departmentExists)
                {
                    return Problem("Invalid departmentId parameter",
                        statusCode: StatusCodes.Status400BadRequest);
                }
            }
            
            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<Doctor> query = dbContext.Users.OfType<Doctor>()
                .Include(d => d.Clinic)
                .Include(d => d.Department)
                .Include(d => d.Favorites)
                .Where(d => d.Status == DoctorStatus.Approved)
                .Where(d => d.Clinic != null)
                .Where(d => d.Clinic.Latitude >= minLat && d.Clinic.Latitude <= maxLat)
                .Where(d => d.Clinic.Longitude >= minLng && d.Clinic.Longitude <= maxLng)
                .Where(d => departmentId == null || d.DepartmentId.ToString() == departmentId);

            List<DoctorMapPinDto> mapPins = await query
                .Select(d => d.ToDoctorMapPinDto(patientId))
                .ToListAsync();

            return Ok(mapPins);
        }


        [Authorize(Roles = $"{Roles.Doctor},{Roles.Patient}")]
        [HttpGet("profile")]
        [ProducesResponseType<DoctorProfileDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile(string? id)
        {
            string? userId = id;

            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            Doctor? doctor = await dbContext.Users.AsNoTracking()
                .OfType<Doctor>()
                .Include(d => d.Department)
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.City)
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.Schedule)
                .FirstOrDefaultAsync(d => d.Id == userId);

            if (doctor == null)
            {
                return NotFound();
            }

            DoctorProfileDto doctorProfile = doctor.ToDoctorProfileDto();

            return Ok(doctorProfile);
        }


        [Authorize(Roles = Roles.Patient)]
        [HttpGet("doctor-details/{id}")]
        [ProducesResponseType<DoctorDetailsDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDoctorDetails(string id)
        {
            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Doctor? doctor = await dbContext.Users.OfType<Doctor>()
                .Include(d => d.Department)
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.Schedule)
                .Include(d => d.Favorites)
                .AsSplitQuery()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

            List<ReviewDto> recentReviews = await dbContext.Reviews
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == id)
                .OrderByDescending(r => r.CreatedAt)
                .Take(3)
                .Select(r => r.ToDto())
                .ToListAsync();

            var stats = await dbContext.Reviews
                .Where(r => r.DoctorId == id)
                .GroupBy(r => 1) // Fake grouping to aggregate all rows
                .Select(g => new
                {
                    AverageRating = g.Average(r => r.Rating),
                    TotalCount = g.Count()
                })
                .FirstOrDefaultAsync();

            int patientCount = await dbContext.Bookings
                .Where(b => b.DoctorId == id && b.Status == BookingStatus.Completed)
                .Select(b => b.PatientId)
                .Distinct()
                .CountAsync();

            DoctorDetailsDto result = doctor.ToDoctorDetailsDto(
                rating: stats?.AverageRating ?? 0,
                reviewCount: stats?.TotalCount ?? 0,
                patientCount: patientCount,
                patientId: patientId,
                reviews: recentReviews);

            return Ok(result);
        }


        [Authorize(Roles = Roles.Doctor)]
        [HttpGet("me/status")]
        [ProducesResponseType<GetDoctorStatusResponseDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyStatus()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var doctorData = await dbContext.Users.AsNoTracking()
                .OfType<Doctor>()
                .Where(d => d.Id == userId)
                .Select(d => new { d.Status })
                .FirstOrDefaultAsync();

            if (doctorData == null)
            {
                return NotFound();
            }

            GetDoctorStatusResponseDto result = new(
                Status: doctorData.Status.ToString(),
                StatusCode: doctorData.Status);

            return Ok(result);
        }


        [Authorize(Roles = Roles.Doctor)]
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMyProfile(
            UpdateDoctorProfileDto updateDoctorProfileDto,
            IValidator<UpdateDoctorProfileDto> validator)
        {
            await validator.ValidateAndThrowAsync(updateDoctorProfileDto);

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Doctor? doctor = await dbContext.Users
                .OfType<Doctor>()
                .Include(d => d.Clinic)
                .Include(d => d.Clinic!.Schedule)
                .FirstOrDefaultAsync(d => d.Id == userId);

            if (doctor == null)
            {
                return NotFound();
            }

            bool departmentExists = await dbContext.Departments.AsNoTracking()
                .AnyAsync(d => d.Id.ToString() == updateDoctorProfileDto.DepartmentId);

            if (!departmentExists)
            {
                return Problem("Invalid department", statusCode: StatusCodes.Status400BadRequest);
            }

            bool cityExists = await dbContext.Cities.AsNoTracking()
                .AnyAsync(c => c.Id.ToString() == updateDoctorProfileDto.Clinic.CityId);

            if (!cityExists)
            {
                return Problem("Invalid city", statusCode: StatusCodes.Status400BadRequest);
            }

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            doctor.UpdateFromDto(updateDoctorProfileDto);

            await userManager.UpdateAsync(doctor);

            Clinic clinic = doctor.Clinic ?? new Clinic
            {
                Id = doctor.Id,
                Name = updateDoctorProfileDto.Clinic.Name,
                Address = updateDoctorProfileDto.Clinic.Address,
                PhoneNumber = updateDoctorProfileDto.Clinic.PhoneNumber
            };

            clinic.UpdateFromDto(updateDoctorProfileDto.Clinic);

            if (doctor.Clinic == null) // If it's a new clinic
            {
                dbContext.Clinics.Add(clinic);
            }

            List<WorkSchedule> existingSchedules = doctor.Clinic?.Schedule.ToList() ?? [];
            dbContext.WorkSchedules.RemoveRange(existingSchedules); // Delete all old entries

            foreach (var scheduleDto in updateDoctorProfileDto.Schedule)
            {
                WorkSchedule newSchedule = scheduleDto.ToEntity(doctor.Id);
                dbContext.WorkSchedules.Add(newSchedule);
            }

            await dbContext.SaveChangesAsync();

            scope.Complete();

            return NoContent();
        }
    }
}
