using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Buses;

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
        public int BusId { get; set; }
        [Required] public string PlateNumber { get; set; } = string.Empty;
        [Required] public string Model { get; set; } = string.Empty;
        [Range(1, 100)] public int TotalSeats { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var bus = await _db.Buses.FindAsync(id);
        if (bus is null)
        {
            return NotFound();
        }

        Input = new InputModel { BusId = bus.BusId, PlateNumber = bus.PlateNumber, Model = bus.Model, TotalSeats = bus.TotalSeats };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var bus = await _db.Buses.Include(b => b.Seats).FirstOrDefaultAsync(b => b.BusId == Input.BusId);
        if (bus is null)
        {
            return NotFound();
        }

        bus.PlateNumber = Input.PlateNumber;
        bus.Model = Input.Model;

        if (Input.TotalSeats > bus.TotalSeats)
        {
            for (var i = bus.TotalSeats + 1; i <= Input.TotalSeats; i++)
            {
                _db.Seats.Add(new Seat { BusId = bus.BusId, SeatNumber = $"S{i}" });
            }
            bus.TotalSeats = Input.TotalSeats;
        }
        else if (Input.TotalSeats < bus.TotalSeats)
        {
            var seatsToRemove = bus.Seats
                .Where(s => int.Parse(s.SeatNumber.TrimStart('S')) > Input.TotalSeats)
                .ToList();

            var seatIdsToRemove = seatsToRemove.Select(s => s.SeatId).ToList();
            if (await _db.Tickets.AnyAsync(t => seatIdsToRemove.Contains(t.SeatId)))
            {
                ModelState.AddModelError(string.Empty, "Can't reduce total seats: some of those seats already have tickets sold.");
                Input.TotalSeats = bus.TotalSeats;
                return Page();
            }

            var holdsToRemove = await _db.SeatHolds.Where(h => seatIdsToRemove.Contains(h.SeatId)).ToListAsync();
            _db.SeatHolds.RemoveRange(holdsToRemove);
            _db.Seats.RemoveRange(seatsToRemove);
            bus.TotalSeats = Input.TotalSeats;
        }

        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "Bus updated.";
        return RedirectToPage("/Admin/Buses/Index");
    }
}
