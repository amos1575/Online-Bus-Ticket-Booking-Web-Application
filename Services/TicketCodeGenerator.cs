using System.Security.Cryptography;

namespace Online_Bus_Ticket_Booking_Web_Application.Services;

public class TicketCodeGenerator : ITicketCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I to avoid confusion

    public string NewCode()
    {
        Span<char> chars = stackalloc char[6];
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return $"BK-{DateTime.UtcNow:yyyyMMdd}-{new string(chars)}";
    }
}
