using CRM.Backend.Domain.Interfaces;
using CRM.Backend.Infra.Auth;
using CRM.Backend.Infra.Persistence;
using CRM.Backend.Infra.Projection;
using CRM.Backend.Infra.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using CRM.Backend.Application.Interfaces;

namespace CRM.Backend.Infra.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "PostgreSQL connection string 'Postgres' is not configured. " +
                "Set it in appsettings.json or via the CONNECTIONSTRINGS__POSTGRES environment variable.");
        services.AddSingleton(new DbConnectionFactory(connectionString));

        services.AddScoped<ICustomerReadRepository, CustomerReadRepository>();
        services.AddScoped<IProjection, CustomerProjection>();
        services.AddScoped<IEventStoreRepository, EventStoreRepository>();
        services.AddScoped<JwtService>();

        services.AddHttpClient<IViaCepService, ViaCepService>()
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetTimeoutPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy() =>
        Policy.TimeoutAsync<HttpResponseMessage>(10);
}