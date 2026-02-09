using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Security.Claims;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.DoctorSystem;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;
using Tabibi.API.Services;

namespace Tabibi.API.Controllers
{
    [Authorize(Roles = Roles.Doctor)]
    [Route("doctor-system")]
    [ApiController]
    public sealed class DoctorSystemController(
        AppDbContext dbContext,
        NotificationService notificationService) : ControllerBase
    {
        [HttpGet("home")]
        [ProducesResponseType<DoctorHomeResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHomeData()
        {
            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            DateTime today = DateTime.UtcNow.Date;

            var doctor = await dbContext.Users.OfType<Doctor>()
                .Where(d => d.Id == doctorId)
                .Select(d => new { d.Name, d.AvatarUrl })
                .FirstOrDefaultAsync();

            if (doctor == null)
            {
                return Unauthorized();
            }

            DoctorHomeStatsDto stats = new(
                TodayCount: await dbContext.Bookings.CountAsync(b =>
                    b.DoctorId == doctorId &&
                    b.AppointmentDate.Date == today &&
                    b.Status == BookingStatus.Confirmed),

                CompletedCount: await dbContext.Bookings.CountAsync(b =>
                    b.DoctorId == doctorId &&
                    b.Status == BookingStatus.Completed),

                CancelledCount: await dbContext.Bookings.CountAsync(b =>
                    b.DoctorId == doctorId &&
                    (b.Status == BookingStatus.Canceled || b.Status == BookingStatus.Refunded))
            );

            List<DoctorAppointmentDto> todayAppointments = await dbContext.Bookings
                .Include(b => b.Patient)
                .Where(b => b.DoctorId == doctorId &&
                            b.AppointmentDate.Date == today &&
                            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed))
                .OrderBy(b => b.AppointmentDate)
                .Take(3)
                .Select(b => b.ToDoctorAppointmentDto())
                .ToListAsync();

            DoctorHomeResponseDto response = new(
                doctor.Name,
                doctor.AvatarUrl,
                Stats: stats,
                todayAppointments
            );

            return Ok(response);
        }


        [HttpGet("schedule")]
        [ProducesResponseType<List<DoctorAppointmentDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDailySchedule(DateTime date)
        {
            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<DoctorAppointmentDto> appointments = await dbContext.Bookings
                .Include(b => b.Patient)
                .Where(b => b.DoctorId == doctorId &&
                            b.AppointmentDate.Date == date.Date &&
                            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed))
                .OrderBy(b => b.AppointmentDate)
                .Select(b => b.ToDoctorAppointmentDto())
                .ToListAsync();

            return Ok(appointments);
        }


        [HttpGet("appointments/{id}")]
        [ProducesResponseType<AppointmentDetailsDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAppointmentDetails(Guid id)
        {
            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Booking? booking = await dbContext.Bookings
                .Include(b => b.Patient)
                    .ThenInclude(p => p.City)
                .Include(b => b.Prescription)
                .FirstOrDefaultAsync(b => b.Id == id && b.DoctorId == doctorId);

            if (booking == null)
            {
                return NotFound();
            }

            AppointmentDetailsDto result = booking.ToAppointmentDetailsDto();

            return Ok(result);
        }


        [HttpPatch("appointments/{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelAppointment(Guid id)
        {
            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Booking? booking = await dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.DoctorId == doctorId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status == BookingStatus.Canceled || booking.Status == BookingStatus.Refunded)
            {
                return BadRequest("Already canceled.");
            }

            if (booking.Status == BookingStatus.Completed)
            {
                return BadRequest("Cannot cancel a completed appointment.");
            }

            if (booking.Status == BookingStatus.Confirmed && !string.IsNullOrEmpty(booking.PaymentIntentId))
            {
                try
                {
                    var refundService = new RefundService();
                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = booking.PaymentIntentId,
                        Reason = RefundReasons.RequestedByCustomer
                    };
                    await refundService.CreateAsync(refundOptions);

                    booking.Status = BookingStatus.Refunded;
                }
                catch (StripeException)
                {
                    return StatusCode(500, "Failed to process refund with Stripe.");
                }
            }
            else
            {
                booking.Status = BookingStatus.Canceled;
            }

            await dbContext.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
                booking.PatientId,
                "Appointment Canceled",
                $"Dr. has canceled your appointment on {booking.AppointmentDate:M}. A refund has been issued.",
                NotificationType.BookingAlert,
                booking.Id
            );

            return Ok();
        }


        [HttpPatch("appointments/{id}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteAppointment(Guid id)
        {
            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Booking? booking = await dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.DoctorId == doctorId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status == BookingStatus.Completed)
            {
                return BadRequest("Already completed.");
            }

            if (booking.Status == BookingStatus.Canceled || booking.Status == BookingStatus.Refunded)
            {
                return BadRequest("Cannot complete a canceled appointment.");
            }

            if (booking.AppointmentDate > DateTime.UtcNow.AddHours(1))
            {
                return BadRequest($"You cannot complete this appointment yet." +
                    $" It is scheduled for {booking.AppointmentDate:f}." +
                    $" You can only complete bookings if the time has passed or is within 1 hour from now.");
            }

            booking.Status = BookingStatus.Completed;
            await dbContext.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
                booking.PatientId,
                "Appointment Completed",
                "Your appointment is complete. Don't forget to leave a review!",
                NotificationType.BookingAlert,
                booking.Id
            );

            return Ok();
        }


        [HttpPost("appointments/{id}/prescription")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddPrescription(
            Guid id,
            [FromBody] CreatePrescriptionDto createPrescriptionDto,
            [FromServices] IValidator<CreatePrescriptionDto> validator)
        {
            await validator.ValidateAndThrowAsync(createPrescriptionDto);

            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Booking? booking = await dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.DoctorId == doctorId);

            if (booking == null)
            {
                return NotFound();
            }

            if (await dbContext.Prescriptions.AnyAsync(p => p.BookingId == id))
            {
                return BadRequest("Prescription already added.");
            }

            Prescription prescription = createPrescriptionDto.ToEntity(id);

            await dbContext.Prescriptions.AddAsync(prescription);
            await dbContext.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
                booking.PatientId,
                "New Prescription",
                "You have received a new prescription. Tap to view.",
                NotificationType.System,
                prescription.Id
            );

            return Ok();
        }


        [HttpPut("prescriptions/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePrescription(
            Guid id,
            [FromBody] CreatePrescriptionDto createPrescriptionDto,
            [FromServices] IValidator<CreatePrescriptionDto> validator)
        {
            await validator.ValidateAndThrowAsync(createPrescriptionDto);

            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Prescription? prescription = await dbContext.Prescriptions
                .Include(p => p.Booking)
                .Include(p => p.Medicines)
                .FirstOrDefaultAsync(p => p.Id == id && p.Booking.DoctorId == doctorId);

            if (prescription == null)
            {
                return NotFound();
            }

            prescription.Diagnosis = createPrescriptionDto.Diagnosis;
            prescription.Notes = createPrescriptionDto.Notes;

            if (prescription.Medicines != null)
            {
                dbContext.PrescriptionItems.RemoveRange(prescription.Medicines);
            }

            List<PrescriptionItem> newMedicines = createPrescriptionDto.Medicines
                .Select(m => m.ToEntity(prescription.Id))
                .ToList();

            await dbContext.PrescriptionItems.AddRangeAsync(newMedicines);

            await dbContext.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
                prescription.Booking?.PatientId,
                "Prescription Updated",
                "Dr. has updated your prescription details.",
                NotificationType.System,
                prescription.Id
            );

            return Ok();
        }


        [HttpDelete("prescriptions/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePrescription(Guid id)
        {
            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Prescription? prescription = await dbContext.Prescriptions
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.Id == id && p.Booking.DoctorId == doctorId);

            if (prescription == null)
            {
                return NotFound();
            }

            dbContext.Prescriptions.Remove(prescription);
            await dbContext.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("earnings")]
        [ProducesResponseType<EarningsPageDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEarnings([FromQuery] string period = "month")
        {
            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            // --- 1. LIFETIME STATS (The Dark Card) ---
            // These numbers don't change when you click the filter tabs

            // A. Lifetime Total
            var lifetimeEarnings = await dbContext.Bookings
                .Where(b => b.DoctorId == doctorId && b.Status == BookingStatus.Completed)
                .SumAsync(b => b.PricePaid);

            // B. Current Month Stats
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var thisMonthEarnings = await dbContext.Bookings
                .Where(b => b.DoctorId == doctorId &&
                            b.Status == BookingStatus.Completed &&
                            b.AppointmentDate >= startOfThisMonth)
                .SumAsync(b => b.PricePaid);

            // C. Commission Logic (Example: 10% Platform Fee)
            decimal commissionRate = 0.10m;
            decimal appCommission = thisMonthEarnings * commissionRate;

            // D. Growth Calculation (This Month vs Last Month)
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);
            var endOfLastMonth = startOfThisMonth.AddTicks(-1);

            var lastMonthEarnings = await dbContext.Bookings
                .Where(b => b.DoctorId == doctorId &&
                            b.Status == BookingStatus.Completed &&
                            b.AppointmentDate >= startOfLastMonth &&
                            b.AppointmentDate <= endOfLastMonth)
                .SumAsync(b => b.PricePaid);

            double growthPercentage = 0;
            if (lastMonthEarnings > 0)
            {
                // Formula: ((New - Old) / Old) * 100
                growthPercentage = (double)((thisMonthEarnings - lastMonthEarnings) / lastMonthEarnings) * 100;
            }
            else if (thisMonthEarnings > 0)
            {
                growthPercentage = 100; // 100% growth if started from 0
            }


            // --- 2. FILTERED DATA (Reactive to Tabs) ---

            DateTime filterStartDate;
            string dateFormat; // How to group chart data (Day vs Month)

            switch (period.ToLower())
            {
                case "week":
                    filterStartDate = now.AddDays(-7); // Last 7 days
                    dateFormat = "ddd"; // Mon, Tue...
                    break;
                case "year":
                    filterStartDate = now.AddYears(-1); // Last 12 months
                    dateFormat = "MMM"; // Jan, Feb...
                    break;
                case "month":
                default:
                    filterStartDate = new DateTime(now.Year, now.Month, 1); // This month
                    dateFormat = "dd MMM"; // 01 Jan, 02 Jan...
                    break;
            }

            var filteredBookings = await dbContext.Bookings
                .Include(b => b.Patient)
                .Where(b => b.DoctorId == doctorId &&
                            b.Status == BookingStatus.Completed &&
                            b.AppointmentDate >= filterStartDate)
                .OrderBy(b => b.AppointmentDate)
                .ToListAsync();

            // E. Quick Stats
            int consultations = filteredBookings.Count;
            decimal avgPerVisit = consultations > 0
                ? filteredBookings.Average(b => b.PricePaid)
                : 0;

            // F. Chart Data (Grouping)
            // Group by Date to create points for the line graph
            var chartData = filteredBookings
                .GroupBy(b => b.AppointmentDate.ToString(dateFormat))
                .Select(g => new ChartDataPointDto(
                    Label: g.Key,
                    Amount: g.Sum(b => b.PricePaid)
                ))
                .ToList();

            // G. Recent Transactions List (Bottom of screen)
            var transactions = filteredBookings
                .OrderByDescending(b => b.AppointmentDate)
                .Take(5)
                .Select(b => new TransactionItemDto(
                    PatientName: b.Patient.Name,
                    Date: b.AppointmentDate.ToString("MMM dd, yyyy"),
                    Amount: b.PricePaid,
                    Status: "Paid"
                ))
                .ToList();

            // --- 3. RETURN FINAL RESPONSE ---
            return Ok(new EarningsPageDto(
                LifetimeEarnings: lifetimeEarnings,
                CurrentMonthEarnings: thisMonthEarnings,
                GrowthPercentage: Math.Round(growthPercentage, 1),
                AppCommission: appCommission,

                TotalConsultations: consultations,
                AveragePerVisit: Math.Round(avgPerVisit, 2),

                ChartData: chartData,
                RecentTransactions: transactions
            ));
        }
    }

    public record EarningsPageDto(
        // The Dark Card (Lifetime / Current Month Stats)
        decimal LifetimeEarnings,
        decimal CurrentMonthEarnings,
        double GrowthPercentage, // e.g. 18.5
        decimal AppCommission,   // Calculated from This Month

        // The Filtered Stats (Reactive to Week/Month/Year)
        int TotalConsultations,
        decimal AveragePerVisit,

        // The Chart Data
        List<ChartDataPointDto> ChartData,

        // The List at the bottom
        List<TransactionItemDto> RecentTransactions
    );

    public record ChartDataPointDto(string Label, decimal Amount); // Label = "Mon", "Tue" or "Week 1"

    public record TransactionItemDto(
        string PatientName,
        string Date,
        decimal Amount,
        string Status // "Paid", "Refunded"
    );
}
