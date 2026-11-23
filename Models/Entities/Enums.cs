namespace Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Expired
}

public enum PaymentMethod
{
    Card,
    MobileMoney
}

public enum PaymentStatus
{
    Pending,
    Success,
    Failed
}
