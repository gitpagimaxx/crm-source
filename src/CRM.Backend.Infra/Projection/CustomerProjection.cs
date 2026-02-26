using CRM.Backend.Domain.Events;
using CRM.Backend.Domain.Interfaces;
using CRM.Backend.Domain.Model;
using CRM.Backend.Infra.Persistence;

namespace CRM.Backend.Infra.Projection;

public class CustomerProjection(ICustomerReadRepository readRepo) : IProjection
{
    private readonly ICustomerReadRepository _readRepo = readRepo;

    public async Task ProjectAsync(DomainEvent domainEvent, CancellationToken ct = default)
    {
        switch (domainEvent)
        {
            case CustomerCreatedEvent e:
                await _readRepo.Upsert(new CustomerReadModel(
                    e.CustomerId, e.CustomerType.ToString(), e.Name, e.Document, e.Email,
                    e.BirthDate, e.CompanyName, e.StateRegistration,
                    e.Address?.ZipCode, e.Address?.Street, e.Address?.Number,
                    e.Address?.Complement, e.Address?.Neighborhood, e.Address?.City, e.Address?.State,
                    "Active", e.OccurredAt, null
                ), ct);
                break;

            case CustomerUpdatedEvent e:
                var existing = await _readRepo.GetById(e.CustomerId, ct);
                if (existing is not null)
                {
                    await _readRepo.Upsert(existing with
                    {
                        Name = e.Name,
                        Email = e.Email,
                        CompanyName = e.CompanyName,
                        StateRegistration = e.StateRegistration,
                        ZipCode = e.Address?.ZipCode ?? existing.ZipCode,
                        Street = e.Address?.Street ?? existing.Street,
                        Number = e.Address?.Number ?? existing.Number,
                        Complement = e.Address?.Complement ?? existing.Complement,
                        Neighborhood = e.Address?.Neighborhood ?? existing.Neighborhood,
                        City = e.Address?.City ?? existing.City,
                        State = e.Address?.State ?? existing.State,
                        UpdatedAt = e.OccurredAt
                    }, ct);
                }
                break;

            case CustomerDeactivatedEvent e:
                var cust = await _readRepo.GetById(e.CustomerId, ct);
                if (cust is not null)
                    await _readRepo.Upsert(cust with { Status = "Inactive", UpdatedAt = e.OccurredAt }, ct);
                break;
        }
    }
}
