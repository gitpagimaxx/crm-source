using CRM.Backend.Application.Models;
using MediatR;

namespace CRM.Backend.Application.Queries.GetCustomers;

public record GetCustomersQuery(int Page = 1, int PageSize = 20) : IRequest<IEnumerable<CustomerDto>>;