using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<BusRoute> Routes => Set<BusRoute>();
    public DbSet<Bus> Buses => Set<Bus>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<SeatHold> SeatHolds => Set<SeatHold>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // EF Core's default key convention looks for "Id" or "{TypeName}Id" (i.e. "BusRouteId"),
        // so the "RouteId" property needs to be configured explicitly.
        builder.Entity<BusRoute>().HasKey(r => r.RouteId);

        // The hard guarantee that a seat can never be sold twice for the same trip.
        builder.Entity<Ticket>()
            .HasIndex(t => new { t.ScheduleId, t.SeatId })
            .IsUnique();

        builder.Entity<Ticket>()
            .HasIndex(t => t.TicketCode)
            .IsUnique();

        builder.Entity<SeatHold>()
            .HasIndex(h => new { h.ScheduleId, h.SeatId });

        builder.Entity<Schedule>()
            .HasOne(s => s.Route)
            .WithMany(r => r.Schedules)
            .HasForeignKey(s => s.RouteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>()
            .HasOne(s => s.Bus)
            .WithMany(bus => bus.Schedules)
            .HasForeignKey(s => s.BusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Seat>()
            .HasOne(s => s.Bus)
            .WithMany(bus => bus.Seats)
            .HasForeignKey(s => s.BusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Booking>()
            .HasOne(b => b.Schedule)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.Booking)
            .WithMany(b => b.Tickets)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Ticket>()
            .HasOne(t => t.Schedule)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.Seat)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Payment>()
            .HasOne(p => p.Booking)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SeatHold>()
            .HasOne(h => h.Schedule)
            .WithMany(s => s.SeatHolds)
            .HasForeignKey(h => h.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SeatHold>()
            .HasOne(h => h.Seat)
            .WithMany()
            .HasForeignKey(h => h.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>().Property(s => s.Price).HasColumnType("decimal(10,2)");
        builder.Entity<Booking>().Property(b => b.TotalAmount).HasColumnType("decimal(10,2)");
        builder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(10,2)");
    }
}
