using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.AdminDashboard;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Route("admin/dashboard")]
    [ApiController]
    public sealed class AdminDashboardController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet("summary")]
        [ProducesResponseType<AdminDashboardSummaryDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary()
        {
            int totalDoctors = await dbContext.Users.OfType<Doctor>().CountAsync();

            int totalPendingDoctors = await dbContext.Users.OfType<Doctor>()
                .CountAsync(d => d.Status == DoctorStatus.Pending);

            int totalApprovedDoctors = await dbContext.Users.OfType<Doctor>()
                .CountAsync(d => d.Status == DoctorStatus.Approved);

            int totalPatients = await dbContext.Users.OfType<Patient>().CountAsync();

            AdminDashboardSummaryDto summaryDto = new()
            {
                TotalDoctors = totalDoctors,
                TotalPendingDoctors = totalPendingDoctors,
                TotalApprovedDoctors = totalApprovedDoctors,
                TotalPatients = totalPatients
            };

            return Ok(summaryDto);
        }
    }
}
