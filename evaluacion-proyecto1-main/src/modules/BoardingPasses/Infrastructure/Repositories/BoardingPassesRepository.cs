using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemadeTiquetess.src.modules.BoardingPasses.Domain.Aggregate;
using SistemadeTiquetess.src.modules.BoardingPasses.Domain.Repositories;
using SistemadeTiquetess.src.modules.BoardingPasses.Infrastructure.Entity;
using SistemadeTiquetess.src.shared.context;

namespace SistemadeTiquetess.src.modules.BoardingPasses.Infrastructure.Repositories;

public class BoardingPassesRepository : IBoardingPassesRepository
{
    private readonly AppDbContext _context;

    public BoardingPassesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(BoardingPass boardingPass)
    {
        var entity = BoardingPassMapper.ToEntity(boardingPass);
        await _context.BoardingPasses.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<BoardingPass?> GetByTicketIdAsync(Guid ticketId)
    {
        var entity = await _context.BoardingPasses
            .FirstOrDefaultAsync(bp => bp.TicketId == ticketId);
        
        return entity == null ? null : BoardingPassMapper.ToDomain(entity);
    }

    public async Task<IEnumerable<BoardingPass>> GetReadyToBoardAsync(Guid flightId)
    {
        var entities = await _context.BoardingPasses
            .Include(bp => bp.Ticket)
            .ThenInclude(t => t.ReservationPassenger)
            .ThenInclude(rp => rp.Reservation)
            .Where(bp => bp.Ticket.ReservationPassenger.Reservation.FlightId == flightId && bp.Status == "Ready to board")
            .ToListAsync();

        return entities.Select(BoardingPassMapper.ToDomain);
    }
}
