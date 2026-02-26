using CRM.Backend.Application.Commands.CreateCustomer;
using CRM.Backend.Application.Common;
using CRM.Backend.Application.Interfaces;
using CRM.Backend.Domain.Aggregates;
using CRM.Backend.Domain.Enums;
using CRM.Backend.Domain.Exceptions;
using CRM.Backend.Domain.Interfaces;
using CRM.Backend.Domain.ValueObjects;
using MediatR;

namespace CRM.Backend.Application.Commands.CreateCustomerr;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly IEventStoreRepository _eventStore;
    private readonly ICustomerReadRepository _readRepo;
    private readonly IViaCepService _viaCep;
    private readonly UserContext _userContext;

    public CreateCustomerCommandHandler(
        IEventStoreRepository eventStore,
        ICustomerReadRepository readRepo,
        IViaCepService viaCep,
        UserContext userContext)
    {
        _eventStore = eventStore;
        _readRepo = readRepo;
        _viaCep = viaCep;
        _userContext = userContext;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var cleanedDoc = new string(request.Document.Where(char.IsDigit).ToArray());
        if (await _readRepo.ExistsByDocument(cleanedDoc, cancellationToken))
            throw new DomainException("Documento já cadastrado.");

        if (await _readRepo.ExistsByEmail(request.Email, cancellationToken))
            throw new DomainException("Email já cadastrado.");

        var customerType = Enum.Parse<CustomerType>(request.CustomerType);

        Address? address = null;
        if (!string.IsNullOrWhiteSpace(request.ZipCode))
        {
            var cepResult = await _viaCep.GetAddressAsync(request.ZipCode, cancellationToken);
            if (cepResult != null && !cepResult.Erro)
            {
                address = new Address(
                    cepResult.Cep,
                    cepResult.Logradouro,
                    request.AddressNumber ?? "",
                    request.AddressComplement ?? cepResult.Complemento,
                    cepResult.Bairro,
                    cepResult.Localidade,
                    cepResult.Uf
                );
            }
        }

        var customerId = Guid.NewGuid();
        var customer = Customer.Create(
            customerId,
            customerType,
            request.Name,
            request.Document,
            request.Email,
            request.BirthDate,
            request.CompanyName,
            request.StateRegistration,
            address
        );

        var metadata = new EventMetadata(
            _userContext.UserId,
            _userContext.Email,
            _userContext.Name,
            _userContext.CorrelationId
        );

        await _eventStore.AppendEventsAsync(customerId, customer.DomainEvents, metadata, cancellationToken);
        customer.ClearDomainEvents();

        return customerId;
    }
}