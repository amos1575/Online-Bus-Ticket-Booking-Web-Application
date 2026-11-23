using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Schedules;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DeleteModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public Schedule Schedule { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var schedule = await _db.Schedules.Include(s => s.Route).Include(s => s.Bus).FirstOrDefaultAsync(s => s.ScheduleId == id);
        if (schedule is null)
        {
            return NotFound();
        }

        Schedule = schedule;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var schedule = await _db.Schedules.FindAsync(id);
        if (schedule is null)
        {
            return NotFound();
        }

        if (await _db.Bookings.AnyAsync(b => b.ScheduleId == id))
        {
            TempData["ErrorMessage"] = "This schedule can't be deleted because it already has bookings.";
            return RedirectToPage("/Admin/Schedules/Index");
        }

        _db.Schedules.Remove(schedule);
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "Schedule deleted.";
        return RedirectToPage("/Admin/Schedules/Index");
    }
}
