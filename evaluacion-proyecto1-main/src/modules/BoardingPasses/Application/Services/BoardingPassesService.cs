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
        if (Guid.TryParse(identifier, out Guid guid))
        {
            // Podría ser el ReservationId o TicketId
            var t = await _context.Tickets
                .Include(t => t.ReservationPassenger)
                    .ThenInclude(rp => rp.Reservation)
                        .ThenInclude(r => r.Flight)
                            .ThenInclude(f => f.Status)
                .FirstOrDefaultAsync(t => t.Id == guid || t.ReservationPassenger.ReservationId == guid);
            if (t != null) return t;
        }

        return await _context.Tickets
            .Include(t => t.ReservationPassenger)
                .ThenInclude(rp => rp.Reservation)
                    .ThenInclude(r => r.Flight)
                        .ThenInclude(f => f.Status)
            .FirstOrDefaultAsync(t => t.TicketNumber == identifier);
    }

    public async Task<BoardingPass> ProcessCheckInAsync(string ticketNumberOrReservationId)
    {
        var ticket = await FindTicketAsync(ticketNumberOrReservationId);

        if (ticket == null)
            throw new Exception("Tiquete o reserva no encontrado.");

        var existingBp = await _repository.GetByTicketIdAsync(ticket.Id);
        if (existingBp != null)
        {
            throw new Exception("El check-in ya fue realizado para este tiquete.");
        }

        // 2. Validar que el tiquete esté emitido o pagado
        if (ticket.StatusId != AppDbContextSeedData.IdTkStIssued && ticket.StatusId != AppDbContextSeedData.IdTkStPagado)
            throw new Exception($"El tiquete no está en estado válido para check-in. (Estado actual ID: {ticket.StatusId})");

        // 3. Validar pagos (se requiere que la reserva esté pagada)
        var payments = await _context.Payments
            .Where(p => p.ReservationId == ticket.ReservationPassenger.ReservationId)
            .ToListAsync();
            
        if (!payments.Any())
            throw new Exception("La reserva asociada al tiquete no ha sido pagada.");

        // 4. Validar que el vuelo esté vigente y habilitado, y tenga puerta y fecha
        var flight = ticket.ReservationPassenger.Reservation.Flight;
        if (flight.Status.Name != "Programado" && flight.Status.Name != "En vuelo")
            throw new Exception($"El vuelo no está habilitado (Estado: {flight.Status.Name}).");

        if (string.IsNullOrEmpty(flight.Gate))
            throw new Exception("El vuelo no tiene una puerta de embarque asignada.");

        if (flight.DepartureTime == default)
            throw new Exception("El vuelo no tiene una fecha y hora de salida asignada.");

        // 5. Generar pase de abordar usando información real del vuelo
        string seat = ticket.ReservationPassenger.SeatNumber;
        if (string.IsNullOrEmpty(seat))
        {
            throw new Exception("El pasajero no tiene un asiento asignado. Asigne uno antes del check-in.");
        }

        DateTime boardingTime = flight.DepartureTime.AddHours(-1); // 1 hora antes de la salida

        var boardingPass = BoardingPass.Create(ticket.Id, flight.Gate, seat, boardingTime);

        // 6. Cambiar estado del tiquete a "Check-in realizado"
        ticket.StatusId = AppDbContextSeedData.IdTkStCheckIn;

        // Guardar cambios
        await _repository.AddAsync(boardingPass);
        
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();

        return boardingPass;
    }
}
