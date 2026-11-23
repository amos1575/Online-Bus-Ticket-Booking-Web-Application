using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Schedules;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> RouteOptions { get; set; } = new();
    public List<SelectListItem> BusOptions { get; set; } = new();

    public class InputModel
    {
        public int ScheduleId { get; set; }
        [Required] public int RouteId { get; set; }
        [Required] public int BusId { get; set; }
        [Required] public DateTime DepartureTime { get; set; }
        [Required] public DateTime ArrivalTime { get; set; }
        [Range(0, 10000000)] public decimal Price { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var schedule = await _db.Schedules.FindAsync(id);
        if (schedule is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            ScheduleId = schedule.ScheduleId,
            RouteId = schedule.RouteId,
            BusId = schedule.BusId,
            DepartureTime = schedule.DepartureTime,
            ArrivalTime = schedule.ArrivalTime,
            Price = schedule.Price
        };
        await LoadOptionsAsync();
        return Page();
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

        var schedule = await _db.Schedules.FindAsync(Input.ScheduleId);
        if (schedule is null)
        {
            return NotFound();
        }

        schedule.RouteId = Input.RouteId;
        schedule.BusId = Input.BusId;
        schedule.DepartureTime = Input.DepartureTime;
        schedule.ArrivalTime = Input.ArrivalTime;
        schedule.Price = Input.Price;
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "Schedule updated.";
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
