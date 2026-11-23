using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;
using Online_Bus_Ticket_Booking_Web_Application.Services;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Booking;

public class HistoryModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookingService _bookingService;

    public HistoryModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IBookingService bookingService)
    {
        _db = db;
        _userManager = userManager;
        _bookingService = bookingService;
    }

    public List<Models.Entities.Booking> Bookings { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        Bookings = await _db.Bookings
            .Include(b => b.Schedule).ThenInclude(s => s.Route)
            .Include(b => b.Tickets).ThenInclude(t => t.Seat)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostCancelAsync(int bookingId)
    {
        var userId = _userManager.GetUserId(User)!;
        var cancelled = await _bookingService.CancelBookingAsync(bookingId, userId);

        TempData[cancelled ? "StatusMessage" : "ErrorMessage"] = cancelled
            ? "Your booking has been cancelled."
            : "That booking can no longer be cancelled.";

        return RedirectToPage();
    }
}
