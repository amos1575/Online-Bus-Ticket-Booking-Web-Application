using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.ViewModels;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Trips;

public class SearchModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public SearchModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string? Origin { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Destination { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? Date { get; set; }

    public List<TripSearchResultViewModel> Results { get; set; } = new();

    public bool HasSearched { get; set; }

    public async Task OnGetAsync()
    {
        HasSearched = !string.IsNullOrWhiteSpace(Origin) || !string.IsNullOrWhiteSpace(Destination) || Date.HasValue;

        var now = DateTime.UtcNow;

        var query = _db.Schedules
            .Include(s => s.Route)
            .Include(s => s.Bus)
            .Where(s => s.DepartureTime > now)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Origin))
        {
            query = query.Where(s => s.Route.Origin.Contains(Origin));
        }

        if (!string.IsNullOrWhiteSpace(Destination))
        {
            query = query.Where(s => s.Route.Destination.Contains(Destination));
        }

        if (Date.HasValue)
        {
            var day = Date.Value.Date;
            query = query.Where(s => s.DepartureTime >= day && s.DepartureTime < day.AddDays(1));
        }

        var schedules = await query
            .OrderBy(s => s.DepartureTime)
            .Select(s => new
            {
                s.ScheduleId,
                s.Route.Origin,
                s.Route.Destination,
                s.DepartureTime,
                s.ArrivalTime,
                s.Price,
                s.Bus.Model,
                s.Bus.PlateNumber,
                s.Bus.TotalSeats,
                SoldCount = s.Tickets.Count,
                HeldCount = s.SeatHolds.Count(h => h.ExpiresAt > now)
            })
            .ToListAsync();

        Results = schedules
            .Select(s => new TripSearchResultViewModel
            {
                ScheduleId = s.ScheduleId,
                Origin = s.Origin,
                Destination = s.Destination,
                DepartureTime = s.DepartureTime,
                ArrivalTime = s.ArrivalTime,
                Price = s.Price,
                BusModel = s.Model,
                PlateNumber = s.PlateNumber,
                TotalSeats = s.TotalSeats,
                FreeSeats = s.TotalSeats - s.SoldCount - s.HeldCount
            })
            .ToList();
    }
}
