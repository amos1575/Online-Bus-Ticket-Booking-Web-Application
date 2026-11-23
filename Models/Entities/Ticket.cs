namespace Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

public class Ticket
{
    public int TicketId { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public int ScheduleId { get; set; }
    public Schedule Schedule { get; set; } = null!;
    public int SeatId { get; set; }
    public Seat Seat { get; set; } = null!;
    public string PassengerName { get; set; } = string.Empty;
    public string TicketCode { get; set; } = string.Empty;
}
