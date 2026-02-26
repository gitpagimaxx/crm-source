using MediatR;

namespace CRM.Backend.Application.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string CustomerType,
    string Name,
    string Document,
    string Email,
    DateTime? BirthDate,
    string? CompanyName,
    string? StateRegistration,
    string? ZipCode,
    string? AddressNumber,
    string? AddressComplement
) : IRequest<Guid>;