using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Schedules;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Schedule> Schedules { get; set; } = new();

    public async Task OnGetAsync()
    {
        Schedules = await _db.Schedules
            .Include(s => s.Route)
            .Include(s => s.Bus)
            .OrderByDescending(s => s.DepartureTime)
            .ToListAsync();
    }
}
