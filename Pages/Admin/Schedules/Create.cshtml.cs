using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Schedules;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> RouteOptions { get; set; } = new();
    public List<SelectListItem> BusOptions { get; set; } = new();

    public class InputModel
    {
        [Required] public int RouteId { get; set; }
        [Required] public int BusId { get; set; }
        [Required] public DateTime DepartureTime { get; set; }
        [Required] public DateTime ArrivalTime { get; set; }
        [Range(0, 10000000)] public decimal Price { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.ArrivalTime <= Input.DepartureTime)
        {
            ModelState.AddModelError(string.Empty, "Arrival time must be after departure time.");
        }

        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        _db.Schedules.Add(new Schedule
        {
            RouteId = Input.RouteId,
            BusId = Input.BusId,
            DepartureTime = Input.DepartureTime,
            ArrivalTime = Input.ArrivalTime,
            Price = Input.Price
        });
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "Schedule added.";
        return RedirectToPage("/Admin/Schedules/Index");
    }

    private async Task LoadOptionsAsync()
    {
        RouteOptions = await _db.Routes
            .OrderBy(r => r.Origin)
            .Select(r => new SelectListItem($"{r.Origin} -> {r.Destination}", r.RouteId.ToString()))
            .ToListAsync();

        BusOptions = await _db.Buses
            .OrderBy(b => b.PlateNumber)
            .Select(b => new SelectListItem($"{b.Model} ({b.PlateNumber})", b.BusId.ToString()))
            .ToListAsync();
    }
}
