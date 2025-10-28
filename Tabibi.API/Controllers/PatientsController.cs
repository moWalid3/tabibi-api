using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tabibi.API.Common;
using Tabibi.API.DTOs.Patients;
using Tabibi.API.Entities;

namespace Tabibi.API.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Route("patients")]
    [ApiController]
    public sealed class PatientsController(UserManager<ApplicationUser> userManager) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            IList<ApplicationUser> patients = await userManager.GetUsersInRoleAsync(Roles.Patient);

            IEnumerable<PatientDto> patientDtos = patients.Select(p => p.ToPatientDto());

            return Ok(new { Items = patientDtos });
        }
    }
}
