namespace Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

public class Seat
{
    public int SeatId { get; set; }
    public int BusId { get; set; }
    public Bus Bus { get; set; } = null!;
    public string SeatNumber { get; set; } = string.Empty;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
