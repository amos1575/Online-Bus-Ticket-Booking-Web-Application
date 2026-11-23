using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Routes;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DeleteModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public BusRoute Route { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route is null)
        {
            return NotFound();
        }

        Route = route;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route is null)
        {
            return NotFound();
        }

        if (await _db.Schedules.AnyAsync(s => s.RouteId == id))
        {
            TempData["ErrorMessage"] = "This route can't be deleted because it has schedules.";
            return RedirectToPage("/Admin/Routes/Index");
        }

        _db.Routes.Remove(route);
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "Route deleted.";
        return RedirectToPage("/Admin/Routes/Index");
    }
}
