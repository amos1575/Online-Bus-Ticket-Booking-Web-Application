using Online_Bus_Ticket_Booking_Web_Application.Models.Entities;

namespace Online_Bus_Ticket_Booking_Web_Application.Services;

public interface IPdfTicketService
{
    byte[] GenerateTicketPdf(Booking booking);
}
