using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Trips;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DetailsModel(ApplicationDbContext db)
    {
        _db = db;
    }

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

    public async Task<IActionResult> OnGetAsync(int scheduleId)
    {
        var now = DateTime.UtcNow;

        var schedule = await _db.Schedules
            .Include(s => s.Route)
            .Include(s => s.Bus)
            .Where(s => s.ScheduleId == scheduleId)
            .Select(s => new
            {
                s.ScheduleId,
                s.Route.Origin,
                s.Route.Destination,
                s.DepartureTime,
                s.ArrivalTime,
                s.Price,
                s.Bus.Model,
                s.Bus.PlateNumber,
                s.Bus.TotalSeats,
                SoldCount = s.Tickets.Count,
                HeldCount = s.SeatHolds.Count(h => h.ExpiresAt > now)
            })
            .FirstOrDefaultAsync();

        if (schedule is null)
        {
            return NotFound();
        }

        ScheduleId = schedule.ScheduleId;
        Origin = schedule.Origin;
        Destination = schedule.Destination;
        DepartureTime = schedule.DepartureTime;
        ArrivalTime = schedule.ArrivalTime;
        Price = schedule.Price;
        BusModel = schedule.Model;
        PlateNumber = schedule.PlateNumber;
        TotalSeats = schedule.TotalSeats;
        FreeSeats = schedule.TotalSeats - schedule.SoldCount - schedule.HeldCount;

        return Page();
    }
}
