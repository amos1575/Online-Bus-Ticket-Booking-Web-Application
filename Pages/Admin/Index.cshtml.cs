using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public int RouteCount { get; set; }
    public int BusCount { get; set; }
    public int ScheduleCount { get; set; }
    public int BookingsToday { get; set; }

    public async Task OnGetAsync()
    {
        RouteCount = await _db.Routes.CountAsync();
        BusCount = await _db.Buses.CountAsync();
        ScheduleCount = await _db.Schedules.CountAsync();

        var today = DateTime.UtcNow.Date;
        BookingsToday = await _db.Bookings.CountAsync(b => b.BookingDate >= today && b.BookingDate < today.AddDays(1) && b.Status != BookingStatus.Cancelled);
    }
}
