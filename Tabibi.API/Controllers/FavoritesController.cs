using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Doctors;
using Tabibi.API.Entities;

namespace Tabibi.API.Controllers
{
    public record AddFavoriteRequest(string DoctorId);

    [Authorize(Roles = Roles.Patient)]
    [Route("favorites")]
    [ApiController]
    public sealed class FavoritesController(AppDbContext dbContext) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Add(AddFavoriteRequest request)
        {
            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            string doctorId = request.DoctorId.ToString();
            bool doctorExists = await dbContext.Doctors
                .AnyAsync(d => d.Id == doctorId);

            if (!doctorExists)
            {
                return NotFound("Doctor not found.");
            }

            bool alreadyFavorited = await dbContext.Favorites
                .AnyAsync(f => f.PatientId == patientId && f.DoctorId == request.DoctorId);

            if (alreadyFavorited)
            {
                return Ok();
            }

            Favorite favorite = new()
            {
                PatientId = patientId,
                DoctorId = request.DoctorId
            };

            await dbContext.Favorites.AddAsync(favorite);
            await dbContext.SaveChangesAsync();

            return Ok();
        }


        [HttpDelete("{doctorId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Remove(string doctorId)
        {
            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Favorite? favorite = await dbContext.Favorites
                .FirstOrDefaultAsync(f => f.PatientId == patientId && f.DoctorId == doctorId);

            if (favorite != null)
            {
                dbContext.Favorites.Remove(favorite);
                await dbContext.SaveChangesAsync();
            }

            return Ok();
        }


        [HttpGet]
        [ProducesResponseType<List<DoctorBasicDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyFavorites()
        {
            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<DoctorBasicDto> favorites = await dbContext.Favorites.AsNoTracking()
                .Where(f => f.PatientId == patientId)
                .Include(f => f.Doctor)
                .Select(f => f.Doctor.ToDoctorBasicDto())
                .ToListAsync();

            return Ok(favorites);
        }
    }
}