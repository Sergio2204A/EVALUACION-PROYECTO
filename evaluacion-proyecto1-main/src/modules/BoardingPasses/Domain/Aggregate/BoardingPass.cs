using System;

namespace SistemadeTiquetess.src.modules.BoardingPasses.Domain.Aggregate;

public class BoardingPass
{
    public Guid Id { get; private set; }
    public string BoardingCode { get; private set; }
    public Guid TicketId { get; private set; }
    public string Gate { get; private set; }
    public string Seat { get; private set; }
    public DateTime BoardingTime { get; private set; }
    public string Status { get; private set; }
    
    private BoardingPass() { }

    private BoardingPass(Guid ticketId, string gate, string seat, DateTime boardingTime)
    {
        Id = Guid.NewGuid();
        BoardingCode = "BP-" + new Random().Next(1000, 9999).ToString();
        TicketId = ticketId;
        Gate = gate;
        Seat = seat;
        BoardingTime = boardingTime;
        Status = "Ready to board";
    }

    public static BoardingPass Create(Guid ticketId, string gate, string seat, DateTime boardingTime)
    {
        return new BoardingPass(ticketId, gate, seat, boardingTime);
    }

    public void MarkAsBoarded()
    {
        Status = "Boarded";
    }
}
