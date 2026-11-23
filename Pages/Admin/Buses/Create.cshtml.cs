using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Buses;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string PlateNumber { get; set; } = string.Empty;
        [Required] public string Model { get; set; } = string.Empty;
        [Range(1, 100)] public int TotalSeats { get; set; } = 40;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var bus = new Bus { PlateNumber = Input.PlateNumber, Model = Input.Model, TotalSeats = Input.TotalSeats };
        _db.Buses.Add(bus);
        await _db.SaveChangesAsync();

        for (var i = 1; i <= bus.TotalSeats; i++)
        {
            _db.Seats.Add(new Seat { BusId = bus.BusId, SeatNumber = $"S{i}" });
        }
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = $"Bus added with {bus.TotalSeats} seats.";
        return RedirectToPage("/Admin/Buses/Index");
    }
}
