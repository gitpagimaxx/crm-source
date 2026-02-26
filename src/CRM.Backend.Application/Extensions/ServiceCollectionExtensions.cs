using CRM.Backend.Application.Commands.CreateCustomer;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Backend.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateCustomerCommand).Assembly));
        services.AddValidatorsFromAssembly(typeof(CreateCustomerCommand).Assembly);
        return services;
    }
}
