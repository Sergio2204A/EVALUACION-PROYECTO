using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SistemadeTiquetess.src.modules.BoardingPasses.Domain.Aggregate;

namespace SistemadeTiquetess.src.modules.BoardingPasses.Domain.Repositories;

public interface IBoardingPassesRepository
{
    Task AddAsync(BoardingPass boardingPass);
    Task<IEnumerable<BoardingPass>> GetReadyToBoardAsync(Guid flightId);
    Task<BoardingPass?> GetByTicketIdAsync(Guid ticketId);
}
