using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Patients;
using Tabibi.API.Entities;

namespace Tabibi.API.Controllers
{
    [Authorize]
    [Route("patients")]
    [ApiController]
    public sealed class PatientsController(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            IList<ApplicationUser> patients = await userManager.GetUsersInRoleAsync(Roles.Patient);

            IEnumerable<PatientDto> patientDtos = patients.Select(p => p.ToDto());

            return Ok(new { Items = patientDtos });
        }

        [HttpGet("me")]
        [EndpointDescription("Get my profile")]
        [ProducesResponseType<PatientProfileDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Patient? patient = await dbContext.Users
                .OfType<Patient>()
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (patient == null)
            {
                return NotFound();
            }

            PatientProfileDto profileDto = patient.ToProfileDto();

            return Ok(profileDto);
        }

        [HttpPut("me")]
        [EndpointDescription("Update my profile. Note: gender: 1 = Male, 2 = Female")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMyProfile(
            UpdatePatientProfileDto updatePatientProfileDto,
            IValidator<UpdatePatientProfileDto> validator)
        {
            await validator.ValidateAndThrowAsync(updatePatientProfileDto);

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Patient? patient = await dbContext.Users
                .OfType<Patient>()
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (patient == null)
            {
                return NotFound();
            }

            if (updatePatientProfileDto.CityId != null)
            {
                bool cityExists = await dbContext.Cities.AsNoTracking()
                    .AnyAsync(c => c.Id.ToString() == updatePatientProfileDto.CityId);

                if (!cityExists)
                {
                    return Problem("Invalid city", statusCode: StatusCodes.Status400BadRequest);
                }
            }

            patient.UpdateFromDto(updatePatientProfileDto);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
