using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Routes;

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
        [Required] public string Origin { get; set; } = string.Empty;
        [Required] public string Destination { get; set; } = string.Empty;
        [Range(0, 100000)] public double DistanceKm { get; set; }
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

        _db.Routes.Add(new BusRoute { Origin = Input.Origin, Destination = Input.Destination, DistanceKm = Input.DistanceKm });
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "Route added.";
        return RedirectToPage("/Admin/Routes/Index");
    }
}
