namespace CRM.Backend.Domain.Events;

public record CustomerDeactivatedEvent : DomainEvent
{
    public Guid CustomerId { get; init; }
    public string Reason { get; init; } = default!;
}