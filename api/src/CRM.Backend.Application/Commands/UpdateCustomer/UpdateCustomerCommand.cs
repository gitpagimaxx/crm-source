using MediatR;

namespace CRM.Backend.Application.Commands.UpdateCustomer;

public record UpdateCustomerCommand(
    Guid CustomerId,
    string Name,
    string Email,
    string? CompanyName,
    string? StateRegistration,
    string? ZipCode,
    string? AddressNumber,
    string? AddressComplement
) : IRequest<Unit>;