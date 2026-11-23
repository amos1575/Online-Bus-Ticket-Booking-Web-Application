namespace Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

public class Bus
{
    public int BusId { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int TotalSeats { get; set; }

    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
