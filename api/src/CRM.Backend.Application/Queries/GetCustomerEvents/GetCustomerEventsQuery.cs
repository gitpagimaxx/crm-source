using CRM.Backend.Application.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRM.Backend.Application.Queries.GetCustomerEvents;

public record GetCustomerEventsQuery(Guid CustomerId) : IRequest<IEnumerable<EventDto>>;