using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SistemadeTiquetess.src.modules.BoardingPasses.Domain.Aggregate;

namespace SistemadeTiquetess.src.modules.BoardingPasses.Application.Interfaces;

public class CheckInDetailsDto
{
    public string PassengerName { get; set; } = string.Empty;
    public string FlightCode { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public string TicketStatus { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
}

public interface IBoardingPassesService
{
    Task<BoardingPass> ProcessCheckInAsync(string ticketNumberOrReservationId);
    Task<BoardingPass?> GetBoardingPassAsync(string ticketNumberOrReservationId);
    Task<IEnumerable<BoardingPass>> GetReadyToBoardAsync(Guid flightId);
    Task<CheckInDetailsDto> GetCheckInDetailsAsync(string ticketNumberOrReservationId);
    Task<bool> ProcessBoardingAsync(string boardingCode);
}
