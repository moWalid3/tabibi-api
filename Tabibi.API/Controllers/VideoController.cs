using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Video;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;
using Tabibi.API.Services;

namespace Tabibi.API.Controllers
{


    [Authorize(Roles = $"{Roles.Doctor},{Roles.Patient}")]
    [Route("video")]
    [ApiController]
    public sealed class VideoController(
        AppDbContext dbContext,
        IConfiguration config) : ControllerBase
    {
        [HttpGet("token/{bookingId}")]
        [ProducesResponseType<VideoCallResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVideoToken(Guid bookingId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Booking? booking = await dbContext.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            string userName = "";

            if (booking.PatientId == userId)
            {
                userName = booking.Patient?.Name;
            }
            else if (booking.DoctorId == userId)
            {
                userName = booking.Doctor?.Name;
            }
            else
            {
                return Forbid();
            }

            if (booking.Type != AppointmentType.VideoCall)
            {
                return BadRequest("This appointment is not a Video Call.");
            }

            if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Completed)
            {
                return BadRequest("Booking must be Confirmed (Paid) to join.");
            }

            // Allow joining 15 minutes before -> until 2 hours after start
            DateTime now = DateTime.UtcNow;
            if (now < booking.AppointmentDate.AddMinutes(-15))
            {
                return BadRequest($"Room opens 15 mins before appointment. Time: {booking.AppointmentDate:t}");
            }
            if (now > booking.AppointmentDate.AddHours(2))
            {
                return BadRequest("Appointment time has expired.");
            }

            long appId = long.Parse(config["ZegoCloud:AppId"]);
            string serverSecret = config["ZegoCloud:ServerSecret"];
            string roomId = booking.Id.ToString();

            long effectiveTime = 3600;

            string token = ZegoTokenGenerator
                .GenerateToken(appId, serverSecret, userId, roomId, effectiveTime);

            VideoCallResponse result = new(
                Token: token,
                RoomId: roomId,
                UserId: userId,
                UserName: userName,
                AppId: appId
            );

            return Ok(result);
        }
    }
}
