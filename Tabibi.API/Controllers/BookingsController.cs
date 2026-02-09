using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Security.Claims;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Bookings;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;
using Tabibi.API.Services;

namespace Tabibi.API.Controllers
{
    [Route("bookings")]
    [ApiController]
    public sealed class BookingsController(
        AppDbContext dbContext,
        NotificationService notificationService) : ControllerBase
    {
        [Authorize(Roles = Roles.Patient)]
        [HttpGet("my-bookings")]
        [ProducesResponseType<List<PatientBookingDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyBookings([FromQuery] string type = "upcoming")
        {
            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<Booking> query = dbContext.Bookings
                .Include(b => b.Doctor)
                    .ThenInclude(d => d.Department)
                .Include(b => b.Doctor)
                    .ThenInclude(d => d.Clinic)
                .Include(b => b.Review)
                .Include(b => b.Prescription)
                .Where(b => b.PatientId == patientId)
                .AsQueryable();

            if (type == "upcoming")
            {
                query = query.Where(b => b.Status == BookingStatus.Confirmed ||
                                         b.Status == BookingStatus.AwaitingPayment)
                             .OrderBy(b => b.AppointmentDate);
            }
            else // completed
            {
                query = query.Where(b => b.Status == BookingStatus.Completed)
                             .OrderByDescending(b => b.AppointmentDate);
            }

            List<PatientBookingDto> list = await query
                .Select(b => b.ToPatientBookingDto())
                .ToListAsync();

            return Ok(list);
        }


        [Authorize(Roles = Roles.Patient)]
        [HttpGet("{bookingId}/prescription")]
        [ProducesResponseType<PrescriptionDetailsDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookingPrescription(Guid bookingId)
        {
            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Prescription? prescription = await dbContext.Prescriptions
                .Include(p => p.Medicines)
                .Include(p => p.Booking)
                .Where(p => p.BookingId == bookingId && p.Booking.PatientId == patientId)
                .FirstOrDefaultAsync();

            if (prescription == null)
            {
                return NotFound("No prescription found for this booking.");
            }

            return Ok(prescription.ToPrescriptionDetailsDto());
        }


        [Authorize(Roles = Roles.Patient)]
        [HttpGet("available-slots")]
        [ProducesResponseType<List<AvailableSlotResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableSlots(string doctorId, DateTime date)
        {
            int dayOfWeek = (int)date.DayOfWeek;

            WorkSchedule? schedule = await dbContext.WorkSchedules
                .FirstOrDefaultAsync(s => s.ClinicId == doctorId && (int)s.DayOfWeek == dayOfWeek);

            if (schedule == null)
            {
                return Ok(new List<AvailableSlotResponseDto>());
            }

            List<DateTime> existingBookings = await dbContext.Bookings
                .Where(b => b.DoctorId == doctorId &&
                            b.AppointmentDate.Date == date.Date &&
                            b.Status != BookingStatus.Canceled &&
                            b.Status != BookingStatus.Refunded)
                .Select(b => b.AppointmentDate)
                .ToListAsync();

            List<AvailableSlotResponseDto> availableSlots = [];

            DateTime currentSlot = date.Date.Add(schedule.OpenTime);
            DateTime workingDayEndTime = date.Date.Add(schedule.CloseTime);

            while (currentSlot < workingDayEndTime)
            {
                bool isTaken = existingBookings.Any(bookedTime =>
                    Math.Abs((bookedTime - currentSlot).TotalMinutes) < 30);

                bool isPast = currentSlot <= DateTime.UtcNow;

                availableSlots.Add(new AvailableSlotResponseDto(
                    StartTime: currentSlot,
                    IsAvailable: !isTaken && !isPast
                ));

                currentSlot = currentSlot.AddMinutes(30);
            }

            return Ok(availableSlots);
        }


        [Authorize(Roles = Roles.Patient)]
        [HttpGet("{id}/resume-payment")]
        [ProducesResponseType<ResumePaymentResponseDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResumeBookingPayment(Guid id)
        {
            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Booking? booking = await dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.PatientId == patientId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status == BookingStatus.Canceled)
            {
                return BadRequest("This booking has expired or was canceled. Please create a new booking.");
            }

            if (booking.Status == BookingStatus.Confirmed)
            {
                return BadRequest("This booking is already paid and confirmed.");
            }

            if (booking.Status != BookingStatus.AwaitingPayment)
            {
                return BadRequest($"Invalid status for payment: {booking.Status}");
            }

            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(booking.PaymentIntentId);

                if (paymentIntent.Status == "canceled")
                {
                    booking.Status = BookingStatus.Canceled;
                    await dbContext.SaveChangesAsync();
                    return BadRequest("The payment session has expired. Please book again.");
                }

                return Ok(new ResumePaymentResponseDto(paymentIntent.ClientSecret));
            }
            catch (StripeException ex)
            {
                return BadRequest($"Unable to retrieve payment details: {ex.Message}");
            }
        }


        [Authorize(Roles = Roles.Patient)]
        [HttpPost]
        [ProducesResponseType<CreateBookingResponseDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateBooking(
            CreateBookingDto createBookingDto,
            IValidator<CreateBookingDto> validator)
        {
            await validator.ValidateAndThrowAsync(createBookingDto);

            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            int dayOfWeek = (int)createBookingDto.AppointmentDate.DayOfWeek;

            WorkSchedule? schedule = await dbContext.WorkSchedules
                .FirstOrDefaultAsync(s => s.ClinicId == createBookingDto.DoctorId &&
                                          (int)s.DayOfWeek == dayOfWeek);

            if (schedule == null)
            {
                return Problem("The doctor is not available on this day.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            TimeSpan requestedTime = createBookingDto.AppointmentDate.TimeOfDay;

            if (requestedTime < schedule.OpenTime || requestedTime >= schedule.CloseTime)
            {
                return Problem("Selected time is outside working hours.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            bool isTaken = await dbContext.Bookings
                .AnyAsync(b => b.DoctorId == createBookingDto.DoctorId &&
                               b.AppointmentDate == createBookingDto.AppointmentDate &&
                               b.Status != BookingStatus.Canceled &&
                               b.Status != BookingStatus.Refunded);

            if (isTaken)
            {
                return Problem("This slot has just been taken.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var doctor = await dbContext.Users.OfType<Doctor>()
                .Where(d => d.Id == createBookingDto.DoctorId)
                .Select(d => new { d.ConsultationFee })
                .FirstOrDefaultAsync();

            if (doctor == null || patientId == null)
            {
                return NotFound();
            }

            var service = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(doctor.ConsultationFee * 100),
                Currency = "egp", // Use "usd" if "egp" gives issues in Test Mode
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = new Dictionary<string, string>
                {
                    { "PatientId", patientId },
                    { "DoctorId", createBookingDto.DoctorId }
                }
            };

            PaymentIntent paymentIntent = await service.CreateAsync(options);

            Booking booking = createBookingDto
                .ToEntity(patientId, doctor.ConsultationFee, paymentIntent.Id);

            await dbContext.Bookings.AddAsync(booking);
            await dbContext.SaveChangesAsync();

            CreateBookingResponseDto response = new(booking.Id, paymentIntent.ClientSecret);

            return Ok(response);
        }


        [Authorize(Roles = Roles.Patient)]
        [HttpPatch("{id}/confirm-payment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmPayment(Guid id)
        {
            Booking? booking = await dbContext.Bookings
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(booking.PaymentIntentId);

            if (paymentIntent.Status != "succeeded")
            {
                return Problem($"Payment not successful. Status: {paymentIntent.Status}",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            // Handling the "16-minute" Race Condition (Zombie Booking)
            if (booking.Status == BookingStatus.Canceled)
            {
                try
                {
                    var refundService = new RefundService();
                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = booking.PaymentIntentId,
                        Reason = RefundReasons.Unknown
                    };
                    await refundService.CreateAsync(refundOptions);

                    return Problem(statusCode: StatusCodes.Status400BadRequest,
                        detail: "This booking session expired before payment was confirmed." +
                        " Your payment has been automatically refunded. Please try booking again.");
                }
                catch (StripeException ex)
                {
                    return StatusCode(500,
                        "Booking expired. Payment was received but auto-refund failed. Please contact support.");
                }
            }

            booking.Status = BookingStatus.Confirmed;
            await dbContext.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
                booking.DoctorId,
                "New Appointment!",
                $"Appointment confirmed: {booking.Patient?.Name} at {booking.AppointmentDate:g}",
                NotificationType.BookingAlert,
                booking.Id
            );

            return Ok();
        }


        [Authorize(Roles = Roles.Patient)]
        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Booking? booking = await dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.PatientId != userId)
            {
                return Forbid();
            }

            if (booking.Status == BookingStatus.AwaitingPayment)
            {
                booking.Status = BookingStatus.Canceled;
                await dbContext.SaveChangesAsync();

                return Ok();
            }

            double hoursUntilAppointment = (booking.AppointmentDate - DateTime.UtcNow).TotalHours;

            if (hoursUntilAppointment < 24)
            {
                booking.Status = BookingStatus.Canceled;
            }
            else
            {
                try
                {
                    var refundService = new RefundService();
                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = booking.PaymentIntentId
                    };
                    await refundService.CreateAsync(refundOptions);

                    booking.Status = BookingStatus.Refunded;
                }
                catch (StripeException ex)
                {
                    return Problem($"Refund failed: {ex.Message}",
                        statusCode: StatusCodes.Status400BadRequest);
                }
            }

            await dbContext.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
                booking.DoctorId,
                "Booking Canceled",
                $"Patient has canceled the appointment on {booking.AppointmentDate:d}",
                NotificationType.BookingAlert,
                booking.Id
            );

            return Ok();
        }
    }
}
