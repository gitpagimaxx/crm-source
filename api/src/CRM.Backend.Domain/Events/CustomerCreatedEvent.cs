using CRM.Backend.Domain.Enums;
using CRM.Backend.Domain.ValueObjects;

namespace CRM.Backend.Domain.Events;

public record CustomerCreatedEvent : DomainEvent
{
    public Guid CustomerId { get; init; }
    public CustomerType CustomerType { get; init; }
    public string Name { get; init; } = default!;
    public string Document { get; init; } = default!;
    public string Email { get; init; } = default!;
    public DateTime? BirthDate { get; init; }
    public string? CompanyName { get; init; }
    public string? StateRegistration { get; init; }
    public Address? Address { get; init; }
}