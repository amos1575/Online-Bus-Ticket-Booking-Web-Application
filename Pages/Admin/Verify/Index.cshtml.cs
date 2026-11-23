using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Verify;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public string? Code { get; set; }

    public Ticket? Ticket { get; set; }
    public bool Searched { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Searched = true;
        if (!string.IsNullOrWhiteSpace(Code))
        {
            var term = Code.Trim();
            Ticket = await _db.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Schedule).ThenInclude(s => s.Route)
                .Include(t => t.Booking)
                .FirstOrDefaultAsync(t => t.TicketCode == term);
        }

        return Page();
    }
}
