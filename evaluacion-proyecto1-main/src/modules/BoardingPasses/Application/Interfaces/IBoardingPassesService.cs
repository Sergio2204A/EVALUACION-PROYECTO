using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SistemadeTiquetess.src.modules.BoardingPasses.Domain.Aggregate;

namespace SistemadeTiquetess.src.modules.BoardingPasses.Application.Interfaces;

public interface IBoardingPassesService
{
    Task<BoardingPass> ProcessCheckInAsync(string ticketNumberOrReservationId);
    Task<BoardingPass?> GetBoardingPassAsync(string ticketNumberOrReservationId);
    Task<IEnumerable<BoardingPass>> GetReadyToBoardAsync(Guid flightId);
}
