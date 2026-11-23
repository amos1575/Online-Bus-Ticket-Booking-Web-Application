using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Data;

public static class SeedData
{
    public const string AdminEmail = "admin@busticket.local";
    public const string AdminPassword = "Admin@12345";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Passenger" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        if (!await db.Routes.AnyAsync())
        {
            var routeA = new BusRoute { Origin = "Kampala", Destination = "Mbarara", DistanceKm = 270 };
            var routeB = new BusRoute { Origin = "Kampala", Destination = "Gulu", DistanceKm = 340 };
            var routeC = new BusRoute { Origin = "Mbarara", Destination = "Kampala", DistanceKm = 270 };
            db.Routes.AddRange(routeA, routeB, routeC);

            var busA = new Bus { PlateNumber = "UBA 123A", Model = "Scania Coach", TotalSeats = 44 };
            var busB = new Bus { PlateNumber = "UBB 456B", Model = "Isuzu Coach", TotalSeats = 32 };
            db.Buses.AddRange(busA, busB);

            await db.SaveChangesAsync();

            foreach (var bus in new[] { busA, busB })
            {
                for (var i = 1; i <= bus.TotalSeats; i++)
                {
                    db.Seats.Add(new Seat { BusId = bus.BusId, SeatNumber = $"S{i}" });
                }
            }

            await db.SaveChangesAsync();

            var today = DateTime.UtcNow.Date;
            db.Schedules.AddRange(
                new Schedule
                {
                    RouteId = routeA.RouteId,
                    BusId = busA.BusId,
                    DepartureTime = today.AddDays(1).AddHours(7),
                    ArrivalTime = today.AddDays(1).AddHours(12),
                    Price = 35000m
                },
                new Schedule
                {
                    RouteId = routeB.RouteId,
                    BusId = busB.BusId,
                    DepartureTime = today.AddDays(1).AddHours(9),
                    ArrivalTime = today.AddDays(1).AddHours(15),
                    Price = 45000m
                },
                new Schedule
                {
                    RouteId = routeC.RouteId,
                    BusId = busA.BusId,
                    DepartureTime = today.AddDays(2).AddHours(8),
                    ArrivalTime = today.AddDays(2).AddHours(13),
                    Price = 35000m
                });

            await db.SaveChangesAsync();
        }
    }
}
