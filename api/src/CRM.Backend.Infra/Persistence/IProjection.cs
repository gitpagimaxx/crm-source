using CRM.Backend.Domain.Events;

namespace CRM.Backend.Infra.Persistence;

public interface IProjection
{
    Task ProjectAsync(DomainEvent domainEvent, CancellationToken ct = default);
}