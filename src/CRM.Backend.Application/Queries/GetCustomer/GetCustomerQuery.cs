using CRM.Backend.Application.Models;
using MediatR;

namespace CRM.Backend.Application.Queries.GetCustomer;

public record GetCustomerQuery(Guid CustomerId) : IRequest<CustomerDto?>;
