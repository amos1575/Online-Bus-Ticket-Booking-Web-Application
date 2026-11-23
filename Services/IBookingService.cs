using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;
using Online_Bus_Ticket_Booking_Web_Application.Models.ViewModels;

namespace Online_Bus_Ticket_Booking_Web_Application.Services;

public record HoldResult(bool Success, List<string> UnavailableSeatNumbers);

public record CheckoutResult(bool Success, string? Error, int? BookingId);

public interface IBookingService
{
    Task<SeatMapViewModel?> GetSeatMapAsync(int scheduleId, string currentUserId);

    Task<HoldResult> CreateHoldsAsync(int scheduleId, List<int> seatIds, string userId);

    Task ReleaseHoldsAsync(int scheduleId, string userId);

    Task<List<SeatHold>> GetActiveHoldsAsync(int scheduleId, string userId);

    Task<CheckoutResult> CompleteCheckoutAsync(
        int scheduleId,
        string userId,
        Dictionary<int, string> passengerNamesBySeatId,
        PaymentMethod method,
        bool simulateFailure);

    Task<bool> CancelBookingAsync(int bookingId, string userId);
}
