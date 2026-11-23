using Microsoft.AspNetCore.Identity;

namespace Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
