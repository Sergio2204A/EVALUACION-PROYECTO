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

public class ReadyToBoardDto
{
    public string BoardingCode { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime BoardingTime { get; set; }
    public DateTime CheckInTime { get; set; }
}

public interface IBoardingPassesService
{
    Task<BoardingPass> ProcessCheckInAsync(string ticketNumberOrReservationId);
    Task<BoardingPass?> GetBoardingPassAsync(string ticketNumberOrReservationId);
    Task<IEnumerable<ReadyToBoardDto>> GetReadyToBoardAsync(Guid flightId, string sortBy = "seat");
    Task<CheckInDetailsDto> GetCheckInDetailsAsync(string ticketNumberOrReservationId);
    Task<bool> ProcessBoardingAsync(string boardingCode);
}
