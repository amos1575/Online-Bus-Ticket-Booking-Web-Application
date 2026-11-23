using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Data;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;
using Online_Bus_Ticket_Booking_Web_Application.Models.ViewModels;

namespace Online_Bus_Ticket_Booking_Web_Application.Pages.Admin.Reports;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<AdminReportRowViewModel> Rows { get; set; } = new();
    public int TotalTicketsSold => Rows.Sum(r => r.TicketsSold);
    public decimal TotalRevenue => Rows.Sum(r => r.Revenue);

    public async Task OnGetAsync()
    {
        var rows = await _db.Schedules
            .Include(s => s.Route)
            .Select(s => new
            {
                s.ScheduleId,
                s.Route.Origin,
                s.Route.Destination,
                s.DepartureTime,
                s.Bus.TotalSeats,
                TicketsSold = s.Tickets.Count,
                Revenue = s.Bookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => (decimal?)b.TotalAmount) ?? 0m
            })
            .OrderByDescending(s => s.DepartureTime)
            .ToListAsync();

        Rows = rows.Select(r => new AdminReportRowViewModel
        {
            ScheduleId = r.ScheduleId,
            Route = $"{r.Origin} -> {r.Destination}",
            DepartureTime = r.DepartureTime,
            TotalSeats = r.TotalSeats,
            TicketsSold = r.TicketsSold,
            Revenue = r.Revenue,
            OccupancyPercent = r.TotalSeats == 0 ? 0 : r.TicketsSold * 100.0 / r.TotalSeats
        }).ToList();
    }
}
