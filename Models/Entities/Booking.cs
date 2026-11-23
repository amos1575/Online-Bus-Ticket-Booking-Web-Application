namespace Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

public class Booking
{
    public int BookingId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int ScheduleId { get; set; }
    public Schedule Schedule { get; set; } = null!;
    public DateTime BookingDate { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public decimal TotalAmount { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
