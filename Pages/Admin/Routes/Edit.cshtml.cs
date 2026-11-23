using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Online_Bus_Ticket_Booking_Web_Application.Data;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Routes;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int RouteId { get; set; }
        [Required] public string Origin { get; set; } = string.Empty;
        [Required] public string Destination { get; set; } = string.Empty;
        [Range(0, 100000)] public double DistanceKm { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route is null)
        {
            return NotFound();
        }

        Input = new InputModel { RouteId = route.RouteId, Origin = route.Origin, Destination = route.Destination, DistanceKm = route.DistanceKm };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var route = await _db.Routes.FindAsync(Input.RouteId);
        if (route is null)
        {
            return NotFound();
        }

        route.Origin = Input.Origin;
        route.Destination = Input.Destination;
        route.DistanceKm = Input.DistanceKm;
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "Route updated.";
        return RedirectToPage("/Admin/Routes/Index");
    }
}
