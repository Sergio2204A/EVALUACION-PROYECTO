using System;

namespace SistemadeTiquetess.src.modules.FlightStatus.Domain.Aggregate;

public class FlightStatus
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    protected FlightStatus() { }

    private FlightStatus(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public static FlightStatus Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del estado de vuelo es requerido.");

        return new FlightStatus(Guid.NewGuid(), name.Trim());
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del estado no puede estar vacío.");

        Name = name.Trim();
    }
}
