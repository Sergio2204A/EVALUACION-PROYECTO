using System;
using System.Reflection;
using SistemadeTiquetess.src.modules.FlightStatus.Domain.Aggregate;

namespace SistemadeTiquetess.src.modules.FlightStatus.Infrastructure.Entity;

public static class StatusMapper
{
    public static StatusEntity ToEntity(Domain.Aggregate.FlightStatus aggregate)
    {
        return new StatusEntity
        {
            Id = aggregate.Id,
            Name = aggregate.Name
        };
    }

    public static Domain.Aggregate.FlightStatus ToDomain(StatusEntity entity)
    {
        var aggregate = (Domain.Aggregate.FlightStatus)Activator.CreateInstance(typeof(Domain.Aggregate.FlightStatus), nonPublic: true)!;
        typeof(Domain.Aggregate.FlightStatus).GetProperty("Id")?.SetValue(aggregate, entity.Id);
        typeof(Domain.Aggregate.FlightStatus).GetProperty("Name")?.SetValue(aggregate, entity.Name);
        return aggregate;
    }
}
