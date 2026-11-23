namespace Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

// Temporary hold on a seat while a passenger is checking out. Expires 10 minutes
// after creation; expired holds are treated as "free" by every availability query.
public class SeatHold
{
    public int SeatHoldId { get; set; }
    public int ScheduleId { get; set; }
    public Schedule Schedule { get; set; } = null!;
    public int SeatId { get; set; }
    public Seat Seat { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int? BookingId { get; set; }
}
