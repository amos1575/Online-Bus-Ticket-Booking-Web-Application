using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Buses;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DeleteModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public Bus Bus { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var bus = await _db.Buses.FindAsync(id);
        if (bus is null)
        {
            return NotFound();
        }

        Bus = bus;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var bus = await _db.Buses.Include(b => b.Seats).FirstOrDefaultAsync(b => b.BusId == id);
        if (bus is null)
        {
            return NotFound();
        }

        if (await _db.Schedules.AnyAsync(s => s.BusId == id))
        {
            TempData["ErrorMessage"] = "This bus can't be deleted because it has schedules.";
            return RedirectToPage("/Admin/Buses/Index");
        }

        _db.Seats.RemoveRange(bus.Seats);
        _db.Buses.Remove(bus);
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "Bus deleted.";
        return RedirectToPage("/Admin/Buses/Index");
    }
}
