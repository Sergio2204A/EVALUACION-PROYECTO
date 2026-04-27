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

    public async Task<IEnumerable<BoardingPass>> GetReadyToBoardAsync(Guid flightId)
    {
        return await _repository.GetReadyToBoardAsync(flightId);
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

        // 1. Verificar que el tiquete no tenga ya el estado Check-in realizado
        var existingBp = await _repository.GetByTicketIdAsync(ticket.Id);
        if (existingBp != null || ticket.StatusId == AppDbContextSeedData.IdTkStCheckIn)
        {
            throw new Exception("El check-in ya fue realizado para este tiquete.");
        }

        // 2. Verificar que el tiquete esté emitido o pagado
        if (ticket.StatusId != AppDbContextSeedData.IdTkStIssued && ticket.StatusId != AppDbContextSeedData.IdTkStPagado)
            throw new Exception($"El tiquete no está en estado emitido o pagado. (Estado actual: {ticket.Status.Name})");

        // 3. Verificar que el pago esté en estado pagado (validamos si hay pagos registrados para la reserva)
        var payments = await _context.Payments
            .Where(p => p.ReservationId == ticket.ReservationPassenger.ReservationId)
            .ToListAsync();
            
        if (!payments.Any())
            throw new Exception("La reserva asociada al tiquete no registra pagos realizados.");

        // 4. Verificar que el vuelo esté vigente y habilitado
        var flight = ticket.ReservationPassenger.Reservation.Flight;
        if (flight.Status.Name != "Programado" && flight.Status.Name != "En vuelo")
            throw new Exception($"El vuelo no está habilitado para check-in (Estado actual: {flight.Status.Name}).");

        // 5. Validar que la fecha y hora del vuelo permitan hacer check-in (ej: máximo 48 horas antes)
        var now = DateTime.UtcNow;
        if (flight.DepartureTime < now)
            throw new Exception("El vuelo ya ha salido.");
        
        if (flight.DepartureTime > now.AddDays(2))
            throw new Exception("El check-in solo está permitido 48 horas antes de la salida.");

        if (string.IsNullOrEmpty(flight.Gate))
            throw new Exception("El vuelo no tiene una puerta de embarque asignada aún.");

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
}
