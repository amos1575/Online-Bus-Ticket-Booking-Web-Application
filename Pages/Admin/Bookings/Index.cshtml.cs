using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Bookings;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<Models.Entities.Booking> Bookings { get; set; } = new();

    public async Task OnGetAsync()
    {
        var query = _db.Bookings
            .Include(b => b.User)
            .Include(b => b.Schedule).ThenInclude(s => s.Route)
            .Include(b => b.Tickets)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Query))
        {
            var term = Query.Trim();
            query = query.Where(b =>
                b.User.FullName.Contains(term) ||
                b.User.Email!.Contains(term) ||
                b.Tickets.Any(t => t.TicketCode.Contains(term)));
        }

        Bookings = await query.OrderByDescending(b => b.BookingDate).Take(200).ToListAsync();
    }
}
