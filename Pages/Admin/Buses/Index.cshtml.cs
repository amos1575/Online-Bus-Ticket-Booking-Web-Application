using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Buses;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Bus> Buses { get; set; } = new();

    public async Task OnGetAsync()
    {
        Buses = await _db.Buses.OrderBy(b => b.PlateNumber).ToListAsync();
    }
}
