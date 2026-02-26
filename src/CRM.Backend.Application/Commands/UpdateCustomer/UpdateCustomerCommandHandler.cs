using CRM.Backend.Application.Common;
using CRM.Backend.Application.Interfaces;
using CRM.Backend.Domain.Aggregates;
using CRM.Backend.Domain.Exceptions;
using CRM.Backend.Domain.Interfaces;
using CRM.Backend.Domain.ValueObjects;
using MediatR;

namespace CRM.Backend.Application.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Unit>
{
    private readonly IEventStoreRepository _eventStore;
    private readonly IViaCepService _viaCep;
    private readonly UserContext _userContext;

    public UpdateCustomerCommandHandler(
        IEventStoreRepository eventStore,
        IViaCepService viaCep,
        UserContext userContext)
    {
        _eventStore = eventStore;
        _viaCep = viaCep;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var events = await _eventStore.LoadEventsAsync(request.CustomerId, cancellationToken);
        if (!events.Any())
            throw new DomainException("Cliente não encontrado.");

        var customer = Customer.Reconstitute(events);

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

        customer.Update(request.Name, request.Email, request.CompanyName, request.StateRegistration, address);

        var metadata = new EventMetadata(
            _userContext.UserId,
            _userContext.Email,
            _userContext.Name,
            _userContext.CorrelationId
        );

        await _eventStore.AppendEventsAsync(customer.Id, customer.DomainEvents, metadata, cancellationToken);
        customer.ClearDomainEvents();

        return Unit.Value;
    }
}