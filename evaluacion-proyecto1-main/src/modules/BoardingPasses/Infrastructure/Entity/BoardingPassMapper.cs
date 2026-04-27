using SistemadeTiquetess.src.modules.BoardingPasses.Domain.Aggregate;
using SistemadeTiquetess.src.modules.BoardingPasses.Infrastructure.Entity;

namespace SistemadeTiquetess.src.modules.BoardingPasses.Infrastructure.Entity;

public static class BoardingPassMapper
{
    public static BoardingPassEntity ToEntity(BoardingPass domain)
    {
        return new BoardingPassEntity
        {
            Id = domain.Id,
            BoardingCode = domain.BoardingCode,
            TicketId = domain.TicketId,
            Gate = domain.Gate,
            Seat = domain.Seat,
            BoardingTime = domain.BoardingTime,
            CheckInTime = domain.CheckInTime,
            Status = domain.Status
        };
    }

    public static BoardingPass ToDomain(BoardingPassEntity entity)
    {
        // Reconstruct using reflection
        var bp = (BoardingPass)Activator.CreateInstance(typeof(BoardingPass), nonPublic: true)!;
        typeof(BoardingPass).GetProperty("Id")?.SetValue(bp, entity.Id);
        typeof(BoardingPass).GetProperty("BoardingCode")?.SetValue(bp, entity.BoardingCode);
        typeof(BoardingPass).GetProperty("TicketId")?.SetValue(bp, entity.TicketId);
        typeof(BoardingPass).GetProperty("Gate")?.SetValue(bp, entity.Gate);
        typeof(BoardingPass).GetProperty("Seat")?.SetValue(bp, entity.Seat);
        typeof(BoardingPass).GetProperty("BoardingTime")?.SetValue(bp, entity.BoardingTime);
        typeof(BoardingPass).GetProperty("CheckInTime")?.SetValue(bp, entity.CheckInTime);
        typeof(BoardingPass).GetProperty("Status")?.SetValue(bp, entity.Status);
        
        return bp;
    }
}
