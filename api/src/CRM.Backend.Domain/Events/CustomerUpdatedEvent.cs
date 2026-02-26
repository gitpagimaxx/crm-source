using CRM.Backend.Domain.ValueObjects;

namespace CRM.Backend.Domain.Events;

public record CustomerUpdatedEvent : DomainEvent
{
    public Guid CustomerId { get; init; }
    public string Name { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string? CompanyName { get; init; }
    public string? StateRegistration { get; init; }
    public Address? Address { get; init; }
}