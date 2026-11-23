using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Online_Bus_Ticket_Booking_Web_Application.Services;

public class PdfTicketService : IPdfTicketService
{
    public byte[] GenerateTicketPdf(Booking booking)
    {
        var schedule = booking.Schedule;
        var route = schedule.Route;
        var bus = schedule.Bus;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("Online Bus Ticket Booking System").FontSize(16).Bold();
                    col.Item().Text("E-Ticket").FontSize(12).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(inner =>
                    {
                        inner.Item().Text($"Booking Code: BK-{booking.BookingId:D6}").Bold();
                        inner.Item().Text($"Route: {route.Origin} -> {route.Destination}");
                        inner.Item().Text($"Departure: {schedule.DepartureTime:ddd, dd MMM yyyy HH:mm}");
                        inner.Item().Text($"Arrival: {schedule.ArrivalTime:ddd, dd MMM yyyy HH:mm}");
                        inner.Item().Text($"Bus: {bus.Model} ({bus.PlateNumber})");
                        inner.Item().Text($"Total Paid: {booking.TotalAmount:C}");
                    });

                    col.Item().Text("Passengers & Seats").Bold().FontSize(13);

                    foreach (var ticket in booking.Tickets)
                    {
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(ticket.PassengerName).Bold();
                                c.Item().Text($"Seat {ticket.Seat.SeatNumber}").FontColor(Colors.Grey.Darken1);
                            });
                            row.ConstantItem(140).AlignRight().Text(ticket.TicketCode).FontSize(10).FontColor(Colors.Blue.Darken2);
                        });
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Please show this ticket, as a download or on your phone, at boarding.").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        return document.GeneratePdf();
    }
}
