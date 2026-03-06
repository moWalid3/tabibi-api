using Microsoft.EntityFrameworkCore;
using Tabibi.API.Database;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.Services
{
    public sealed class AppointmentReminderJob(
        AppDbContext dbContext,
        IFcmNotificationService fcmService,
        ILogger<AppointmentReminderJob> logger)
    {
        public async Task Send2HourReminder(Guid bookingId)
        {
            logger.LogInformation($"Executing 2-hour reminder for Booking: {bookingId}");
            await ProcessReminder(bookingId, "2 hours");
        }

        public async Task Send10MinuteReminder(Guid bookingId)
        {
            logger.LogInformation($"Executing 10-minute reminder for Booking: {bookingId}");
            await ProcessReminder(bookingId, "10 minutes");
        }

        private async Task ProcessReminder(Guid bookingId, string timeRemaining)
        {
            Booking? booking = await dbContext.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return;
            }

            if (booking.Status == BookingStatus.Canceled ||
                booking.Status == BookingStatus.Completed ||
                booking.Status == BookingStatus.Refunded)
            {
                logger.LogInformation($"Booking {bookingId} status is {booking.Status}. Skipping {timeRemaining} reminder.");
                return;
            }

            string appointmentTypeString = booking.Type == AppointmentType.VideoCall ? "Video Call" : "Clinic Visit";

            if (!string.IsNullOrEmpty(booking.Patient?.FcmToken))
            {
                string patientTitle = $"Appointment in {timeRemaining}!";
                string patientBody = $"Reminder: Your {appointmentTypeString} with Dr. {booking.Doctor?.Name} starts in {timeRemaining}.";

                var patientData = new Dictionary<string, string>
                {
                    { "type", "booking_reminder" },
                    { "bookingId", booking.Id.ToString() },
                    { "appointmentType", booking.Type.ToString() },
                    { "role", "patient" }
                };

                await fcmService.SendPushNotificationAsync
                    (booking.Patient.FcmToken, patientTitle, patientBody, patientData);
            }

            if (!string.IsNullOrEmpty(booking.Doctor?.FcmToken))
            {
                string doctorTitle = "Upcoming Appointment";
                string doctorBody = $"Your {appointmentTypeString} with {booking.Patient?.Name} starts in {timeRemaining}.";

                var doctorData = new Dictionary<string, string>
                {
                    { "type", "booking_reminder" },
                    { "bookingId", booking.Id.ToString() },
                    { "appointmentType", booking.Type.ToString() },
                    { "role", "doctor" }
                };

                await fcmService.SendPushNotificationAsync
                    (booking.Doctor.FcmToken, doctorTitle, doctorBody, doctorData);
            }
        }
    }
}
