using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;
using Online_Bus_Ticket_Booking_Web_Application.Services;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Booking;

public class TicketDownloadModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPdfTicketService _pdfTicketService;

    public TicketDownloadModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IPdfTicketService pdfTicketService)
    {
        _db = db;
        _userManager = userManager;
        _pdfTicketService = pdfTicketService;
    }

    public async Task<IActionResult> OnGetAsync(int bookingId)
    {
        var userId = _userManager.GetUserId(User)!;

        var booking = await _db.Bookings
            .Include(b => b.Schedule).ThenInclude(s => s.Route)
            .Include(b => b.Schedule).ThenInclude(s => s.Bus)
            .Include(b => b.Tickets).ThenInclude(t => t.Seat)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

        if (booking is null)
        {
            return NotFound();
        }

        var pdfBytes = _pdfTicketService.GenerateTicketPdf(booking);
        return File(pdfBytes, "application/pdf", $"ticket-{booking.BookingId}.pdf");
    }
}
