
using Microsoft.EntityFrameworkCore;
using Stripe;
using Tabibi.API.Database;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;

namespace Tabibi.API.Services
{
    public sealed class BookingCleanupService(
        IServiceProvider serviceProvider,
        ILogger<BookingCleanupService> logger) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<BookingCleanupService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run this loop until the app stops
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredBookings();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up bookings.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ProcessExpiredBookings()
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var expirationTime = DateTime.UtcNow.AddMinutes(-15);

            List<Booking> expiredBookings = await context.Bookings
                .Where(b => b.Status == BookingStatus.AwaitingPayment &&
                            b.CreatedAt < expirationTime)
                .ToListAsync();

            if (expiredBookings.Count != 0)
            {
                foreach (var booking in expiredBookings)
                {
                    booking.Status = BookingStatus.Canceled;

                    try
                    {
                        var service = new PaymentIntentService();
                        await service.CancelAsync(booking.PaymentIntentId);
                    }
                    catch
                    {
                        // Ignore errors (e.g., if user already paid or it's invalid)
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"Released {expiredBookings.Count} expired bookings.");
            }
        }
    }
}
