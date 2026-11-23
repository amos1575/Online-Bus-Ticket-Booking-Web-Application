using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Services;

// Simulated payment gateway for demo purposes. Always succeeds unless the caller
// explicitly asks to simulate a failure (checkout exposes this as a demo toggle).
public class FakePaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(decimal amount, PaymentMethod method, bool simulateFailure)
    {
        if (simulateFailure)
        {
            return Task.FromResult(new PaymentResult(false, "The payment gateway declined the transaction."));
        }

        return Task.FromResult(new PaymentResult(true, null));
    }
}
