using CRM.Backend.Application.Models;
using CRM.Backend.Domain.Interfaces;
using MediatR;

namespace CRM.Backend.Application.Queries.GetCustomers;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, IEnumerable<CustomerDto>>
{
    private readonly ICustomerReadRepository _readRepo;

    public GetCustomersQueryHandler(ICustomerReadRepository readRepo) => _readRepo = readRepo;

    public async Task<IEnumerable<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var models = await _readRepo.GetAll(request.Page, request.PageSize, cancellationToken);
        return models.Select(m => new CustomerDto(
            m.Id, m.CustomerType, m.Name, m.Document, m.Email,
            m.BirthDate, m.CompanyName, m.StateRegistration,
            m.ZipCode is null ? null : new AddressDto(m.ZipCode, m.Street, m.Number, m.Complement, m.Neighborhood, m.City, m.State),
            m.Status, m.CreatedAt, m.UpdatedAt
        ));
    }
}