using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Routes;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<BusRoute> Routes { get; set; } = new();

    public async Task OnGetAsync()
    {
        Routes = await _db.Routes.OrderBy(r => r.Origin).ThenBy(r => r.Destination).ToListAsync();
    }
}
