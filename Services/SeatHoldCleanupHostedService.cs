using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Services;

// Housekeeping only: every availability query already filters out expired SeatHolds at read
// time, so this is not required for correctness. It just keeps the table small and flips
// abandoned Pending bookings to Expired so admin reports stay accurate.
public class SeatHoldCleanupHostedService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(2);
    private readonly IServiceScopeFactory _scopeFactory;

    public SeatHoldCleanupHostedService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // app shutting down
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var now = DateTime.UtcNow;
        var expiredHolds = await db.SeatHolds.Where(h => h.ExpiresAt <= now).ToListAsync(ct);
        if (expiredHolds.Count == 0)
        {
            return;
        }

        var bookingIds = expiredHolds.Where(h => h.BookingId.HasValue).Select(h => h.BookingId!.Value).Distinct().ToList();
        db.SeatHolds.RemoveRange(expiredHolds);
        await db.SaveChangesAsync(ct);

        if (bookingIds.Count > 0)
        {
            var pendingBookings = await db.Bookings
                .Where(b => bookingIds.Contains(b.BookingId) && b.Status == BookingStatus.Pending)
                .ToListAsync(ct);

            foreach (var booking in pendingBookings)
            {
                var hasTickets = await db.Tickets.AnyAsync(t => t.BookingId == booking.BookingId, ct);
                booking.Status = hasTickets ? BookingStatus.Confirmed : BookingStatus.Expired;
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
