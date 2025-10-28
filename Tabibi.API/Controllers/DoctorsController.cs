using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Doctors;
using Tabibi.API.Entities;

namespace Tabibi.API.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Route("doctors")]
    [ApiController]
    public sealed class DoctorsController(AppDbContext dbContext, UserManager<ApplicationUser> userManager) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            IList<ApplicationUser> doctors = await userManager.GetUsersInRoleAsync(Roles.Doctor);

            IEnumerable<DoctorDto> doctorDtos = doctors.Select(d => d.ToDoctorDto());

            return Ok(new { Items = doctorDtos });
        }
    }
}
