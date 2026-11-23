namespace Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

public class Schedule
{
    public int ScheduleId { get; set; }
    public int RouteId { get; set; }
    public BusRoute Route { get; set; } = null!;
    public int BusId { get; set; }
    public Bus Bus { get; set; } = null!;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<SeatHold> SeatHolds { get; set; } = new List<SeatHold>();
}
