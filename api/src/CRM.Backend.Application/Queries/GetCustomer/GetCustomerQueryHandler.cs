using CRM.Backend.Application.Models;
using CRM.Backend.Domain.Interfaces;
using CRM.Backend.Domain.Model;
using MediatR;

namespace CRM.Backend.Application.Queries.GetCustomer;

public class GetCustomerQueryHandler(ICustomerReadRepository readRepo) : IRequestHandler<GetCustomerQuery, CustomerDto?>
{
    private readonly ICustomerReadRepository _readRepo = readRepo;

    public async Task<CustomerDto?> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var model = await _readRepo.GetById(request.CustomerId, cancellationToken);
        if (model is null) return null;
        return MapToDto(model);
    }

    private static CustomerDto MapToDto(CustomerReadModel m) => new(
        m.Id, m.CustomerType, m.Name, m.Document, m.Email,
        m.BirthDate, m.CompanyName, m.StateRegistration,
        m.ZipCode is null ? null : new AddressDto(m.ZipCode, m.Street, m.Number, m.Complement, m.Neighborhood, m.City, m.State),
        m.Status, m.CreatedAt, m.UpdatedAt
    );
}