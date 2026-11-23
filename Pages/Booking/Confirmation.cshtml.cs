using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Booking;

public class ConfirmationModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ConfirmationModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public Models.Entities.Booking Booking { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int bookingId)
    {
        var userId = _userManager.GetUserId(User)!;

        var booking = await _db.Bookings
            .Include(b => b.Schedule).ThenInclude(s => s.Route)
            .Include(b => b.Tickets).ThenInclude(t => t.Seat)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

        if (booking is null)
        {
            return NotFound();
        }

        Booking = booking;
        return Page();
    }
}
