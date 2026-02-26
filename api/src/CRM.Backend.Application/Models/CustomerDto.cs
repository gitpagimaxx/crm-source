namespace CRM.Backend.Application.Models;

public record CustomerDto(
    Guid Id,
    string CustomerType,
    string Name,
    string Document,
    string Email,
    DateTime? BirthDate,
    string? CompanyName,
    string? StateRegistration,
    AddressDto? Address,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record AddressDto(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State
);