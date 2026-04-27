using System.Threading.Tasks;
using SistemadeTiquetess.src.modules.FlightStatus.Domain.Aggregate;
using SistemadeTiquetess.src.modules.FlightStatus.Domain.Repositories;

namespace SistemadeTiquetess.src.modules.FlightStatus.Application.Usecase;

public class CreateStatusUseCase
{
    private readonly IFlightStatusRepository _repository;
    public CreateStatusUseCase(IFlightStatusRepository repository) => _repository = repository;
    public async Task<FlightStatus> ExecuteAsync(FlightStatus status)
    {
        await _repository.AddAsync(status);
        return status;
    }
}
