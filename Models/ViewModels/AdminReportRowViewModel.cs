namespace Online_Bus_Ticket_Booking_Web_Application.Models.ViewModels;

public class AdminReportRowViewModel
{
    public int ScheduleId { get; set; }
    public string Route { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public int TotalSeats { get; set; }
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
    public double OccupancyPercent { get; set; }
}
