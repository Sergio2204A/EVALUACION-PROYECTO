using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemadeTiquetess.src.modules.BoardingPasses.Application.Interfaces;
using SistemadeTiquetess.src.modules.BoardingPasses.Domain.Aggregate;
using SistemadeTiquetess.src.modules.BoardingPasses.Domain.Repositories;
using SistemadeTiquetess.src.shared.context;

namespace SistemadeTiquetess.src.modules.BoardingPasses.Application.Services;

public class BoardingPassesService : IBoardingPassesService
{
    private readonly IBoardingPassesRepository _repository;
    private readonly AppDbContext _context;

    public BoardingPassesService(IBoardingPassesRepository repository, AppDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<IEnumerable<ReadyToBoardDto>> GetReadyToBoardAsync(Guid flightId, string sortBy = "seat")
    {
        var entities = await _context.BoardingPasses
            .Include(bp => bp.Ticket)
                .ThenInclude(t => t.ReservationPassenger)
                    .ThenInclude(rp => rp.Customer)
            .Include(bp => bp.Ticket)
                .ThenInclude(t => t.ReservationPassenger)
                    .ThenInclude(rp => rp.Reservation)
            .Where(bp => bp.Ticket.ReservationPassenger.Reservation.FlightId == flightId && bp.Status == "Ready to board")
            .ToListAsync();

        var query = entities.Select(bp => new ReadyToBoardDto
        {
            BoardingCode = bp.BoardingCode,
            PassengerName = bp.Ticket.ReservationPassenger.Customer.FullName,
            DocumentNumber = bp.Ticket.ReservationPassenger.Customer.DocumentNumber,
            SeatNumber = bp.Seat,
            TicketNumber = bp.Ticket.TicketNumber,
            Status = bp.Status,
            BoardingTime = bp.BoardingTime,
            CheckInTime = bp.CheckInTime
        });

        if (sortBy == "time")
            return query.OrderBy(q => q.CheckInTime);
        
        return query.OrderBy(q => q.SeatNumber);
    }

    public async Task<BoardingPass?> GetBoardingPassAsync(string ticketNumberOrReservationId)
    {
        var ticket = await FindTicketAsync(ticketNumberOrReservationId);
        if (ticket == null) return null;
        
        return await _repository.GetByTicketIdAsync(ticket.Id);
    }

    private async Task<Tickets.Infrastructure.Entity.TicketEntity?> FindTicketAsync(string identifier)
    {
        var query = _context.Tickets
            .Include(t => t.Status)
            .Include(t => t.ReservationPassenger)
                .ThenInclude(rp => rp.Customer)
            .Include(t => t.ReservationPassenger)
                .ThenInclude(rp => rp.Reservation)
                    .ThenInclude(r => r.Flight)
                        .ThenInclude(f => f.Status);

        if (Guid.TryParse(identifier, out Guid guid))
        {
            return await query.FirstOrDefaultAsync(t => t.Id == guid || t.ReservationPassenger.ReservationId == guid);
        }

        return await query.FirstOrDefaultAsync(t => t.TicketNumber == identifier);
    }

    public async Task<CheckInDetailsDto> GetCheckInDetailsAsync(string identifier)
    {
        var ticket = await FindTicketAsync(identifier);
        if (ticket == null) throw new Exception("Tiquete o reserva no encontrado.");

        var flight = ticket.ReservationPassenger.Reservation.Flight;
        
        var segment = await _context.Segments
            .Include(s => s.OriginAirport)
            .Include(s => s.DestinationAirport)
            .FirstOrDefaultAsync(s => s.FlightId == flight.Id);

        return new CheckInDetailsDto
        {
            PassengerName = ticket.ReservationPassenger.Customer.FullName,
            FlightCode = flight.FlightNumber,
            Origin = segment?.OriginAirport.Name ?? "N/A",
            Destination = segment?.DestinationAirport.Name ?? "N/A",
            DepartureTime = flight.DepartureTime,
            TicketStatus = ticket.Status.Name,
            SeatNumber = string.IsNullOrEmpty(ticket.ReservationPassenger.SeatNumber) ? "No asignado" : ticket.ReservationPassenger.SeatNumber,
            TicketNumber = ticket.TicketNumber
        };
    }

    public async Task<BoardingPass> ProcessCheckInAsync(string ticketNumberOrReservationId)
    {
        var ticket = await FindTicketAsync(ticketNumberOrReservationId);

        if (ticket == null)
            throw new Exception("Tiquete o reserva no encontrado.");

        // 1. Verificar si el check-in ya fue realizado
        var existingBp = await _repository.GetByTicketIdAsync(ticket.Id);
        if (existingBp != null || ticket.StatusId == AppDbContextSeedData.IdTkStCheckIn)
        {
            throw new Exception("Error: check-in ya realizado.");
        }

        // 2. Verificar que el tiquete esté emitido
        // Nota: Si el estado es "Cancelado", mostrar mensaje correspondiente
        if (ticket.Status.Name == "Cancelado")
            throw new Exception("Error: tiquete cancelado.");

        if (ticket.StatusId != AppDbContextSeedData.IdTkStIssued && ticket.StatusId != AppDbContextSeedData.IdTkStPagado && ticket.StatusId != AppDbContextSeedData.IdTkStCheckIn)
            throw new Exception("Error: tiquete no emitido.");

        // 3. Verificar pagos (pago pendiente)
        var payments = await _context.Payments
            .Where(p => p.ReservationId == ticket.ReservationPassenger.ReservationId)
            .ToListAsync();
            
        if (!payments.Any())
            throw new Exception("Error: pago pendiente.");

        // 4. Verificar estado del vuelo
        var flight = ticket.ReservationPassenger.Reservation.Flight;
        
        if (flight.Status.Name == "Cancelado")
            throw new Exception("Error: vuelo cancelado.");

        if (flight.Status.Name != "Programado" && flight.Status.Name != "En vuelo")
            throw new Exception("Error: vuelo no habilitado.");

        // 5. Verificar tiempo permitido (fuera del tiempo permitido)
        var now = DateTime.UtcNow;
        if (flight.DepartureTime < now)
            throw new Exception("Error: fuera del tiempo permitido (el vuelo ya salió).");
        
        if (flight.DepartureTime > now.AddDays(2))
            throw new Exception("Error: fuera del tiempo permitido (muy temprano para check-in).");

        if (string.IsNullOrEmpty(flight.Gate))
            throw new Exception("Error: vuelo no habilitado (puerta no asignada).");

        // 6. Confirmar que el pasajero tenga asiento asignado; si no lo tiene, asignarlo
        string seat = ticket.ReservationPassenger.SeatNumber;
        if (string.IsNullOrEmpty(seat))
        {
            var availableSeat = await _context.SeatAvailabilities
                .Include(sa => sa.Seat)
                .FirstOrDefaultAsync(sa => sa.FlightId == flight.Id && sa.IsAvailable);
            
            if (availableSeat == null)
                throw new Exception("No hay asientos disponibles para asignar en este vuelo.");
            
            availableSeat.IsAvailable = false;
            ticket.ReservationPassenger.SeatNumber = availableSeat.Seat.SeatNumber;
            seat = availableSeat.Seat.SeatNumber;
            
            _context.SeatAvailabilities.Update(availableSeat);
            _context.ReservationPassengers.Update(ticket.ReservationPassenger);
        }

        DateTime boardingTime = flight.DepartureTime.AddHours(-1); // 1 hora antes de la salida

        var boardingPass = BoardingPass.Create(ticket.Id, flight.Gate, seat, boardingTime);

        // 7. Cambiar estado del tiquete a "Check-in realizado"
        ticket.StatusId = AppDbContextSeedData.IdTkStCheckIn;

        // Guardar cambios
        await _repository.AddAsync(boardingPass);
        _context.Tickets.Update(ticket);
        
        await _context.SaveChangesAsync();

        return boardingPass;
    }

    public async Task<bool> ProcessBoardingAsync(string boardingCode)
    {
        var boardingPass = await _repository.GetByCodeAsync(boardingCode);
        if (boardingPass == null)
            throw new Exception("Pase de abordar no encontrado.");

        if (boardingPass.Status != "Ready to board")
            throw new Exception($"El pase de abordar no está en estado para abordar (Estado: {boardingPass.Status}).");

        var ticket = await _context.Tickets.FindAsync(boardingPass.TicketId);
        if (ticket == null)
            throw new Exception("Tiquete asociado no encontrado.");

        // Marcar como abordado
        boardingPass.MarkAsBoarded();
        ticket.StatusId = AppDbContextSeedData.IdTkStAbordado;

        await _repository.UpdateAsync(boardingPass);
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();

        return true;
    }
}
