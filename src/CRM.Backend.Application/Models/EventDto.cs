namespace CRM.Backend.Application.Models;

public record EventDto(
    long Id,
    Guid StreamId,
    string EventType,
    string EventData,
    int StreamVersion,
    DateTime CreatedAt,
    string? ActorUserId,
    string? ActorEmail,
    string? ActorName,
    string? CorrelationId
);