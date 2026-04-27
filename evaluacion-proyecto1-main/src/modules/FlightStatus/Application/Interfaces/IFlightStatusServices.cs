using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SistemadeTiquetess.src.modules.FlightStatus.Domain.Aggregate;

namespace SistemadeTiquetess.src.modules.FlightStatus.Application.Interfaces;

public interface IFlightStatusServices
{
    Task<IEnumerable<FlightStatus>> GetAllAsync();
    Task<FlightStatus?> GetByIdAsync(Guid id);
    Task<FlightStatus> CreateAsync(FlightStatus status);
    Task UpdateAsync(FlightStatus status);
    Task DeleteAsync(Guid id);
}
