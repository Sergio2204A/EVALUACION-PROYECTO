using System;
using SistemadeTiquetess.src.modules.Tickets.Infrastructure.Entity;

namespace SistemadeTiquetess.src.modules.BoardingPasses.Infrastructure.Entity;

public class BoardingPassEntity
{
    public Guid Id { get; set; }
    public string BoardingCode { get; set; } = string.Empty;
    public Guid TicketId { get; set; }
    public string Gate { get; set; } = string.Empty;
    public string Seat { get; set; } = string.Empty;
    public DateTime BoardingTime { get; set; }
    public DateTime CheckInTime { get; set; }
    public string Status { get; set; } = string.Empty; // e.g. "Boarding", "Ready"

    // Navigation property
    public virtual TicketEntity Ticket { get; set; } = null!;
}
