namespace Online_Bus_Ticket_Booking_Web_Application.Models.ViewModels;

public class TripSearchResultViewModel
{
    public int ScheduleId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public string BusModel { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int FreeSeats { get; set; }
}

public enum SeatStatus
{
    Available,
    HeldByOthers,
    Sold,
    SelectedByMe
}

public class SeatViewModel
{
    public int SeatId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public SeatStatus Status { get; set; }
}

public class SeatMapViewModel
{
    public int ScheduleId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public decimal Price { get; set; }
    public List<SeatViewModel> Seats { get; set; } = new();
}
