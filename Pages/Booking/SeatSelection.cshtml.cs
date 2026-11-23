using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;
using Online_Bus_Ticket_Booking_Web_Application.Models.ViewModels;
using Online_Bus_Ticket_Booking_Web_Application.Services;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Booking;

public class SeatSelectionModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SeatSelectionModel(IBookingService bookingService, UserManager<ApplicationUser> userManager)
    {
        _bookingService = bookingService;
        _userManager = userManager;
    }

    public SeatMapViewModel SeatMap { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int scheduleId)
    {
        var userId = _userManager.GetUserId(User)!;
        var seatMap = await _bookingService.GetSeatMapAsync(scheduleId, userId);
        if (seatMap is null)
        {
            return NotFound();
        }

        SeatMap = seatMap;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int scheduleId, List<int> selectedSeatIds)
    {
        var userId = _userManager.GetUserId(User)!;

        if (selectedSeatIds.Count == 0)
        {
            ErrorMessage = "Please select at least one seat.";
            var seatMap = await _bookingService.GetSeatMapAsync(scheduleId, userId);
            SeatMap = seatMap ?? new SeatMapViewModel();
            return Page();
        }

        var result = await _bookingService.CreateHoldsAsync(scheduleId, selectedSeatIds, userId);
        if (!result.Success)
        {
            ErrorMessage = result.UnavailableSeatNumbers.Count > 0
                ? $"These seats just became unavailable: {string.Join(", ", result.UnavailableSeatNumbers)}. Please choose different seats."
                : "Please select at least one seat.";
            var seatMap = await _bookingService.GetSeatMapAsync(scheduleId, userId);
            SeatMap = seatMap ?? new SeatMapViewModel();
            return Page();
        }

        return RedirectToPage("/Booking/Checkout", new { scheduleId });
    }
}
