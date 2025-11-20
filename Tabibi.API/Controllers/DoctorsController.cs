using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Transactions;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Doctors;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.Controllers
{
    [Authorize(Roles = Roles.Doctor)]
    [Route("doctors")]
    [ApiController]
    public sealed class DoctorsController(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext) : ControllerBase
    {
        [HttpGet("me")]
        [ProducesResponseType<DoctorProfileDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Doctor? doctor = await dbContext.Users
                .OfType<Doctor>()
                .Include(d => d.Clinic)
                .ThenInclude(c => c!.Schedule)
                .FirstOrDefaultAsync(d => d.Id == userId && d.Status == DoctorStatus.Approved);

            if (doctor == null)
            {
                return NotFound();
            }

            DoctorProfileDto doctorProfile = doctor.ToDoctorProfileDto();

            return Ok(doctorProfile);
        }

        [HttpGet("me/status")]
        [ProducesResponseType<GetDoctorStatusResponseDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyStatus()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var doctorData = await dbContext.Users
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
