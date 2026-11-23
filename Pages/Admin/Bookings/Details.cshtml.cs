using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Bookings;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DetailsModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public Models.Entities.Booking Booking { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.User)
            .Include(b => b.Schedule).ThenInclude(s => s.Route)
            .Include(b => b.Tickets).ThenInclude(t => t.Seat)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking is null)
        {
            return NotFound();
        }

        Booking = booking;
        return Page();
    }
}
