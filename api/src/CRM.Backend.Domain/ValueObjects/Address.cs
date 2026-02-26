namespace CRM.Backend.Domain.ValueObjects;

public record Address(
    string ZipCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State
);