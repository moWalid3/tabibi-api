using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.MedicalProfile;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.Controllers
{
    [Route("medical-profile")]
    [ApiController]
    public sealed class MedicalProfileController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet("me")]
        [Authorize(Roles = Roles.Patient)]
        [ProducesResponseType<MedicalProfileDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyProfile()
        {
            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            MedicalProfile? profile = await dbContext.MedicalProfiles
                .FirstOrDefaultAsync(m => m.PatientId == patientId);

            if (profile == null)
            {
                return Ok(new MedicalProfileDto(patientId!, [], "", "", [], null, null, false, DateTime.UtcNow));
            }

            return Ok(profile.ToDto());
        }


        [HttpPut("me")]
        [Authorize(Roles = Roles.Patient)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateMyProfile(
            UpdateMedicalProfileDto dto,
            IValidator<UpdateMedicalProfileDto> validator)
        {
            await validator.ValidateAndThrowAsync(dto);

            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            MedicalProfile? profile = await dbContext.MedicalProfiles
                .FirstOrDefaultAsync(m => m.PatientId == patientId);

            if (profile == null)
            {
                profile = new MedicalProfile { PatientId = patientId! };
                dbContext.MedicalProfiles.Add(profile);
            }

            profile.ChronicDiseases = dto.ChronicDiseases ?? [];
            profile.Surgeries = dto.Surgeries ?? [];
            profile.Medications = dto.Medications;
            profile.Allergies = dto.Allergies;
            profile.Weight = dto.Weight;
            profile.Height = dto.Height;
            profile.UpdatedAt = DateTime.UtcNow;
            profile.IsCompleted = true;

            await dbContext.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("patients/{patientId}")]
        [Authorize(Roles = Roles.Doctor)]
        [ProducesResponseType<MedicalProfileDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPatientProfileForDoctor(string patientId)
        {
            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool hasAppointment = await dbContext.Bookings
                .AnyAsync(b => b.DoctorId == doctorId &&
                               b.PatientId == patientId &&
                               (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed));

            if (!hasAppointment)
            {
                return Forbid("You do not have permission to view this patient's medical profile.");
            }

            MedicalProfile? profile = await dbContext.MedicalProfiles
                .FirstOrDefaultAsync(m => m.PatientId == patientId);

            if (profile == null)
            {
                return NotFound("Patient has not filled out their medical profile.");
            }

            return Ok(profile.ToDto());
        }
    }
}
