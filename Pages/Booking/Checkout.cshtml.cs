using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;
using Online_Bus_Ticket_Booking_Web_Application.Services;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Booking;

public class CheckoutModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public CheckoutModel(IBookingService bookingService, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        _bookingService = bookingService;
        _userManager = userManager;
        _db = db;
    }

    public int ScheduleId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public decimal PricePerSeat { get; set; }
    public List<SeatHold> Holds { get; set; } = new();
    public DateTime? EarliestExpiry { get; set; }
    public decimal TotalAmount => PricePerSeat * Holds.Count;
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int scheduleId)
    {
        var userId = _userManager.GetUserId(User)!;
        var loaded = await LoadAsync(scheduleId, userId);
        if (!loaded)
        {
            TempData["ErrorMessage"] = "Your seat hold has expired. Please select your seats again.";
            return RedirectToPage("/Booking/SeatSelection", new { scheduleId });
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int scheduleId, Dictionary<int, string> passengerNames, PaymentMethod method, bool simulateFailure)
    {
        var userId = _userManager.GetUserId(User)!;

        var result = await _bookingService.CompleteCheckoutAsync(scheduleId, userId, passengerNames, method, simulateFailure);
        if (!result.Success)
        {
            var loaded = await LoadAsync(scheduleId, userId);
            if (!loaded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToPage("/Booking/SeatSelection", new { scheduleId });
            }

            ErrorMessage = result.Error;
            return Page();
        }

        return RedirectToPage("/Booking/Confirmation", new { bookingId = result.BookingId });
    }

    private async Task<bool> LoadAsync(int scheduleId, string userId)
    {
        var holds = await _bookingService.GetActiveHoldsAsync(scheduleId, userId);
        if (holds.Count == 0)
        {
            return false;
        }

        var schedule = await _db.Schedules.Include(s => s.Route).FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);
        if (schedule is null)
        {
            return false;
        }

        ScheduleId = scheduleId;
        Origin = schedule.Route.Origin;
        Destination = schedule.Route.Destination;
        DepartureTime = schedule.DepartureTime;
        PricePerSeat = schedule.Price;
        Holds = holds;
        EarliestExpiry = holds.Min(h => h.ExpiresAt);
        return true;
    }
}
