using CRM.Backend.Application.Models;
using CRM.Backend.Domain.Interfaces;
using MediatR;

namespace CRM.Backend.Application.Queries.GetCustomerEvents;

public class GetCustomerEventsQueryHandler(IEventStoreRepository eventStore) : IRequestHandler<GetCustomerEventsQuery, IEnumerable<EventDto>>
{
    private readonly IEventStoreRepository _eventStore = eventStore;

    public async Task<IEnumerable<EventDto>> Handle(GetCustomerEventsQuery request, CancellationToken cancellationToken)
    {
        var storedEvents = await _eventStore.GetStoredEventsAsync(request.CustomerId, cancellationToken);
        return storedEvents.Select(e => new EventDto(
            e.Id, e.StreamId, e.EventType, e.EventData,
            e.StreamVersion, e.CreatedAt,
            e.ActorUserId, e.ActorEmail, e.ActorName, e.CorrelationId
        ));
    }
}