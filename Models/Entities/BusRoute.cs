namespace Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

// Named BusRoute (not "Route") to avoid colliding with Microsoft.AspNetCore.Routing.Route,
// which is a global using in this project. Maps to the "Routes" table.
public class BusRoute
{
    public int RouteId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public double DistanceKm { get; set; }

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
