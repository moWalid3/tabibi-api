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

            TimeZoneInfo egyptZone;
            try
            {
                egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); // Fallback for Linux/Docker
            }

            DateTime egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptZone);

            DateTime startOfEgyptToday = egyptNow.Date;
            DateTime endOfEgyptToday = startOfEgyptToday.AddDays(1).AddTicks(-1);

            DateTime startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfEgyptToday, egyptZone);
            DateTime endOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(endOfEgyptToday, egyptZone);

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
                    b.AppointmentDate >= startOfDayUtc &&
                    b.AppointmentDate <= endOfDayUtc &&
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
                            b.AppointmentDate >= startOfDayUtc &&
                            b.AppointmentDate <= endOfDayUtc &&
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

            TimeZoneInfo egyptZone;
            try
            {
                egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); // Fallback for Linux/Docker
            }

            DateTime requestedDateEgypt = new(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);

            DateTime startOfEgyptDay = requestedDateEgypt;
            DateTime endOfEgyptDay = startOfEgyptDay.AddDays(1).AddTicks(-1);

            DateTime startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfEgyptDay, egyptZone);
            DateTime endOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(endOfEgyptDay, egyptZone);

            List<DoctorAppointmentDto> appointments = await dbContext.Bookings
                .Include(b => b.Patient)
                .Where(b => b.DoctorId == doctorId &&
                            b.AppointmentDate >= startOfDayUtc &&
                            b.AppointmentDate <= endOfDayUtc &&
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
                .Include(b => b.Doctor)
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
                $"Dr. {booking.Doctor?.Name} has canceled your appointment on {booking.AppointmentDate:M}. A refund has been issued.",
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
                .Include(b => b.Doctor)
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
                $"You have received a new prescription from Dr. {booking.Doctor?.Name}. Tap to view.",
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
                    .ThenInclude(b => b.Doctor)
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
                $"Dr. {prescription.Booking?.Doctor?.Name} has updated your prescription details on {prescription.Booking?.AppointmentDate:d}.",
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
                    .ThenInclude(b => b.Doctor)
                .FirstOrDefaultAsync(p => p.Id == id && p.Booking.DoctorId == doctorId);

            if (prescription == null)
            {
                return NotFound();
            }

            dbContext.Prescriptions.Remove(prescription);
            await dbContext.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
                prescription.Booking?.PatientId,
                "Prescription Deleted",
                $"Dr. {prescription.Booking?.Doctor?.Name} has deleted your prescription on {prescription?.Booking?.AppointmentDate:d}.",
                NotificationType.System,
                prescription.Id
            );

            return Ok();
        }


        [HttpGet("earnings/summary")]
        [ProducesResponseType<EarningsSummaryDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEarningsSummary()
        {
            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            DateTime now = DateTime.UtcNow;

            IQueryable<Booking> completedBookings = dbContext.Bookings
                .Where(b => b.DoctorId == doctorId && b.Status == BookingStatus.Completed);

            decimal totalEarnings = await completedBookings.SumAsync(b => b.PricePaid);

            int totalConsultations = await completedBookings.CountAsync();
            decimal averagePerVisit = totalConsultations > 0
                ? totalEarnings / totalConsultations
                : 0;

            DateTime startOfThisMonth = new(now.Year, now.Month, 1);
            decimal thisMonthEarnings = await completedBookings
                .Where(b => b.AppointmentDate >= startOfThisMonth)
                .SumAsync(b => b.PricePaid);

            DateTime startOfLastMonth = startOfThisMonth.AddMonths(-1);
            decimal lastMonthEarnings = await completedBookings
                .Where(b => b.AppointmentDate >= startOfLastMonth && b.AppointmentDate < startOfThisMonth)
                .SumAsync(b => b.PricePaid);

            double growth = 0;
            if (lastMonthEarnings > 0)
            {
                growth = (double)((thisMonthEarnings - lastMonthEarnings) / lastMonthEarnings) * 100;
            }
            else if (thisMonthEarnings > 0)
            {
                growth = 100;
            }

            decimal commissionRate = 0.10m;

            DateTime weekStartDate = now.AddDays(-7);

            List<Booking> weeklyData = await completedBookings
                .Where(b => b.AppointmentDate >= weekStartDate)
                .ToListAsync();

            List<ChartDataPointDto> chartData = weeklyData
                .GroupBy(b => b.AppointmentDate.ToString("ddd")) // "Mon", "Tue"
                .Select(g => new ChartDataPointDto(g.Key, g.Sum(b => b.PricePaid)))
                .ToList();

            List<TransactionItemDto> recentTransactions = await dbContext.Bookings
                .Include(b => b.Patient)
                .Where(b => b.DoctorId == doctorId && b.Status == BookingStatus.Completed)
                .OrderByDescending(b => b.AppointmentDate)
                .Take(3)
                .Select(b => b.ToTransactionItemDto())
                .ToListAsync();

            EarningsSummaryDto result = new(
                TotalLifetimeEarnings: totalEarnings,
                ThisMonthEarnings: thisMonthEarnings,
                AppCommission: thisMonthEarnings * commissionRate,
                GrowthPercentage: Math.Round(growth, 1),
                TotalConsultations: totalConsultations,
                AveragePerVisit: Math.Round(averagePerVisit, 2),
                WeeklyChartData: chartData,
                RecentTransactions: recentTransactions
            );

            return Ok(result);
        }


        [HttpGet("earnings/analytics")]
        [ProducesResponseType<List<ChartDataPointDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEarningsAnalytics([FromQuery] string period = "month")
        {
            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            DateTime now = DateTime.UtcNow;

            DateTime startDate;
            string dateFormat;

            switch (period.ToLower())
            {
                case "week":
                    startDate = now.AddDays(-7);
                    dateFormat = "ddd";
                    break;
                case "year":
                    startDate = now.AddYears(-1);
                    dateFormat = "MMM";
                    break;
                case "month":
                default:
                    startDate = new DateTime(now.Year, now.Month, 1);
                    dateFormat = "dd MMM";
                    break;
            }

            List<Booking> bookings = await dbContext.Bookings
                .Where(b => b.DoctorId == doctorId &&
                            b.Status == BookingStatus.Completed &&
                            b.AppointmentDate >= startDate)
                .ToListAsync();

            List<ChartDataPointDto> chartData = bookings
                .GroupBy(b => b.AppointmentDate.ToString(dateFormat))
                .Select(g => new ChartDataPointDto(g.Key, g.Sum(b => b.PricePaid)))
                .ToList();

            return Ok(chartData);
        }


        [HttpGet("transactions")]
        [ProducesResponseType<PaginationResult<TransactionItemDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactions(
            int page = 1,
            int pageSize = 10)
        {
            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IOrderedQueryable<Booking> query = dbContext.Bookings
                .Include(b => b.Patient)
                .Where(b => b.DoctorId == doctorId && b.Status == BookingStatus.Completed)
                .OrderByDescending(b => b.AppointmentDate);

            int totalCount = await query.CountAsync();

            List<TransactionItemDto> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => b.ToTransactionItemDto())
                .ToListAsync();

            PaginationResult<TransactionItemDto> result = new()
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(result);
        }
    }
}
