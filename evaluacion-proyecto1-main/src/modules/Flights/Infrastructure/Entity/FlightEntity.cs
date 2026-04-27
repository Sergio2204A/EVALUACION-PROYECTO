using System;
using SistemadeTiquetess.src.modules.FlightStatus.Infrastructure.Entity;

namespace SistemadeTiquetess.src.modules.Flights.Infrastructure.Entity;

public class FlightEntity
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public Guid StatusId { get; set; }
    public DateTime DepartureTime { get; set; }
    public string Gate { get; set; } = string.Empty;

    // Propiedad de navegación
    public virtual StatusEntity Status { get; set; } = null!;
}
