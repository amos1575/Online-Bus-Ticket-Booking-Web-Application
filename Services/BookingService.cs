using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;
using Online_Bus_Ticket_Booking_Web_Application.Models.ViewModels;

namespace Online_Bus_Ticket_Booking_Web_Application.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _db;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ITicketCodeGenerator _codeGenerator;

    public BookingService(ApplicationDbContext db, IPaymentGateway paymentGateway, ITicketCodeGenerator codeGenerator)
    {
        _db = db;
        _paymentGateway = paymentGateway;
        _codeGenerator = codeGenerator;
    }

    public async Task<SeatMapViewModel?> GetSeatMapAsync(int scheduleId, string currentUserId)
    {
        var schedule = await _db.Schedules
            .Include(s => s.Route)
            .Include(s => s.Bus).ThenInclude(b => b.Seats)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

        if (schedule is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        var soldSeatIds = await _db.Tickets
            .Where(t => t.ScheduleId == scheduleId)
            .Select(t => t.SeatId)
            .ToListAsync();

        var activeHolds = await _db.SeatHolds
            .Where(h => h.ScheduleId == scheduleId && h.ExpiresAt > now)
            .Select(h => new { h.SeatId, h.UserId })
            .ToListAsync();

        var heldByOthers = activeHolds.Where(h => h.UserId != currentUserId).Select(h => h.SeatId).ToHashSet();
        var heldByMe = activeHolds.Where(h => h.UserId == currentUserId).Select(h => h.SeatId).ToHashSet();
        var sold = soldSeatIds.ToHashSet();

        var seats = schedule.Bus.Seats
            .OrderBy(s => s.SeatNumber)
            .Select(seat => new SeatViewModel
            {
                SeatId = seat.SeatId,
                SeatNumber = seat.SeatNumber,
                Status = sold.Contains(seat.SeatId) ? SeatStatus.Sold
                    : heldByMe.Contains(seat.SeatId) ? SeatStatus.SelectedByMe
                    : heldByOthers.Contains(seat.SeatId) ? SeatStatus.HeldByOthers
                    : SeatStatus.Available
            })
            .ToList();

        return new SeatMapViewModel
        {
            ScheduleId = schedule.ScheduleId,
            Origin = schedule.Route.Origin,
            Destination = schedule.Route.Destination,
            DepartureTime = schedule.DepartureTime,
            Price = schedule.Price,
            Seats = seats
        };
    }

    public async Task<HoldResult> CreateHoldsAsync(int scheduleId, List<int> seatIds, string userId)
    {
        if (seatIds.Count == 0)
        {
            return new HoldResult(false, new List<string>());
        }

        var schedule = await _db.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);
        if (schedule is null)
        {
            return new HoldResult(false, new List<string>());
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        // Clear out this user's own previous holds for this schedule so re-selecting seats
        // doesn't leave stale rows behind.
        await ReleaseHoldsInternalAsync(scheduleId, userId);

        var now = DateTime.UtcNow;

        var seats = await _db.Seats
            .Where(s => s.BusId == schedule.BusId && seatIds.Contains(s.SeatId))
            .ToListAsync();

        var soldSeatIds = await _db.Tickets
            .Where(t => t.ScheduleId == scheduleId && seatIds.Contains(t.SeatId))
            .Select(t => t.SeatId)
            .ToListAsync();

        var heldByOthersSeatIds = await _db.SeatHolds
            .Where(h => h.ScheduleId == scheduleId && h.ExpiresAt > now && h.UserId != userId && seatIds.Contains(h.SeatId))
            .Select(h => h.SeatId)
            .ToListAsync();

        var unavailableSeatIds = soldSeatIds.Concat(heldByOthersSeatIds).ToHashSet();

        if (unavailableSeatIds.Count > 0)
        {
            await transaction.RollbackAsync();
            var names = seats.Where(s => unavailableSeatIds.Contains(s.SeatId)).Select(s => s.SeatNumber).ToList();
            return new HoldResult(false, names);
        }

        var booking = new Booking
        {
            UserId = userId,
            ScheduleId = scheduleId,
            BookingDate = now,
            Status = BookingStatus.Pending,
            TotalAmount = schedule.Price * seatIds.Count
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        foreach (var seatId in seatIds)
        {
            _db.SeatHolds.Add(new SeatHold
            {
                ScheduleId = scheduleId,
                SeatId = seatId,
                UserId = userId,
                ExpiresAt = now.AddMinutes(10),
                BookingId = booking.BookingId
            });
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new HoldResult(true, new List<string>());
    }

    public async Task ReleaseHoldsAsync(int scheduleId, string userId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        await ReleaseHoldsInternalAsync(scheduleId, userId);
        await transaction.CommitAsync();
    }

    private async Task ReleaseHoldsInternalAsync(int scheduleId, string userId)
    {
        var holds = await _db.SeatHolds
            .Where(h => h.ScheduleId == scheduleId && h.UserId == userId)
            .ToListAsync();

        if (holds.Count == 0)
        {
            return;
        }

        var bookingIds = holds.Where(h => h.BookingId.HasValue).Select(h => h.BookingId!.Value).Distinct().ToList();
        _db.SeatHolds.RemoveRange(holds);
        await _db.SaveChangesAsync();

        if (bookingIds.Count > 0)
        {
            var pendingBookings = await _db.Bookings
                .Where(b => bookingIds.Contains(b.BookingId) && b.Status == BookingStatus.Pending)
                .ToListAsync();

            foreach (var booking in pendingBookings)
            {
                var hasTickets = await _db.Tickets.AnyAsync(t => t.BookingId == booking.BookingId);
                if (!hasTickets)
                {
                    _db.Bookings.Remove(booking); // cascades to any failed Payment attempts
                }
            }

            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<SeatHold>> GetActiveHoldsAsync(int scheduleId, string userId)
    {
        var now = DateTime.UtcNow;
        return await _db.SeatHolds
            .Include(h => h.Seat)
            .Where(h => h.ScheduleId == scheduleId && h.UserId == userId && h.ExpiresAt > now)
            .OrderBy(h => h.Seat.SeatNumber)
            .ToListAsync();
    }

    public async Task<CheckoutResult> CompleteCheckoutAsync(
        int scheduleId,
        string userId,
        Dictionary<int, string> passengerNamesBySeatId,
        PaymentMethod method,
        bool simulateFailure)
    {
        var now = DateTime.UtcNow;

        var holds = await _db.SeatHolds
            .Include(h => h.Seat)
            .Where(h => h.ScheduleId == scheduleId && h.UserId == userId && h.ExpiresAt > now)
            .ToListAsync();

        if (holds.Count == 0)
        {
            return new CheckoutResult(false, "Your seat hold has expired. Please select your seats again.", null);
        }

        var bookingId = holds[0].BookingId;
        if (bookingId is null || holds.Any(h => h.BookingId != bookingId))
        {
            return new CheckoutResult(false, "Something went wrong with your booking. Please select your seats again.", null);
        }

        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId.Value && b.Status == BookingStatus.Pending);
        if (booking is null)
        {
            return new CheckoutResult(false, "Your booking could not be found. Please start again.", null);
        }

        if (!holds.All(h => passengerNamesBySeatId.ContainsKey(h.SeatId) && !string.IsNullOrWhiteSpace(passengerNamesBySeatId[h.SeatId])))
        {
            return new CheckoutResult(false, "Please enter a passenger name for every seat.", null);
        }

        var payment = await _paymentGateway.ChargeAsync(booking.TotalAmount, method, simulateFailure);

        await using var transaction = await _db.Database.BeginTransactionAsync();

        _db.Payments.Add(new Payment
        {
            BookingId = booking.BookingId,
            Amount = booking.TotalAmount,
            Method = method,
            Status = payment.Success ? PaymentStatus.Success : PaymentStatus.Failed,
            PaidAt = payment.Success ? now : null
        });

        if (!payment.Success)
        {
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new CheckoutResult(false, payment.FailureReason ?? "Payment failed. Please try again.", null);
        }

        foreach (var hold in holds)
        {
            _db.Tickets.Add(new Ticket
            {
                BookingId = booking.BookingId,
                ScheduleId = scheduleId,
                SeatId = hold.SeatId,
                PassengerName = passengerNamesBySeatId[hold.SeatId].Trim(),
                TicketCode = _codeGenerator.NewCode()
            });
        }

        booking.Status = BookingStatus.Confirmed;
        _db.SeatHolds.RemoveRange(holds);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // The unique index on Tickets(ScheduleId, SeatId) is the final backstop: if another
            // user's checkout raced past the hold check and already sold one of these seats,
            // this insert fails here and we roll back cleanly instead of double-selling the seat.
            await transaction.RollbackAsync();
            return new CheckoutResult(false, "Sorry, one of your seats was just taken by another passenger. Please pick a different seat.", null);
        }

        await transaction.CommitAsync();
        return new CheckoutResult(true, null, booking.BookingId);
    }

    public async Task<bool> CancelBookingAsync(int bookingId, string userId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Schedule)
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

        if (booking is null)
        {
            return false;
        }

        if (booking.Status is not (BookingStatus.Confirmed or BookingStatus.Pending))
        {
            return false;
        }

        if (booking.Schedule.DepartureTime <= DateTime.UtcNow)
        {
            return false;
        }

        booking.Status = BookingStatus.Cancelled;
        _db.Tickets.RemoveRange(booking.Tickets); // frees the seats (releases the unique index rows)

        await _db.SaveChangesAsync();
        return true;
    }
}
