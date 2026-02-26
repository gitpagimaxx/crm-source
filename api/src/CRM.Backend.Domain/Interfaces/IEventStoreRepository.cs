using CRM.Backend.Domain.Events;

namespace CRM.Backend.Domain.Interfaces;

public interface IEventStoreRepository
{
    Task AppendEventsAsync(Guid streamId, IEnumerable<DomainEvent> events, EventMetadata metadata, CancellationToken ct = default);
    Task<IEnumerable<DomainEvent>> LoadEventsAsync(Guid streamId, CancellationToken ct = default);
    Task<IEnumerable<StoredEvent>> GetStoredEventsAsync(Guid streamId, CancellationToken ct = default);
}

public record EventMetadata(
    string? ActorUserId,
    string? ActorEmail,
    string? ActorName,
    string? CorrelationId
);

public record StoredEvent(
    long Id,
    Guid StreamId,
    string EventType,
    string EventData,
    string Metadata,
    int StreamVersion,
    DateTime CreatedAt,
    string? ActorUserId,
    string? ActorEmail,
    string? ActorName,
    string? CorrelationId
);