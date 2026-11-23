using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Services;

public record PaymentResult(bool Success, string? FailureReason);

// Abstraction over the real payment provider (mobile money / card). The proposal calls for
// an external payment gateway; this interface lets a real provider be plugged in later
// without touching booking logic.
public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(decimal amount, PaymentMethod method, bool simulateFailure);
}
