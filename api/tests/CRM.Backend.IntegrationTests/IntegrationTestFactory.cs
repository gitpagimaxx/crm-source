using CRM.Backend.Application.Common;
using CRM.Backend.Application.Interfaces;
using CRM.Backend.Domain.Interfaces;
using CRM.Backend.Infra.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace CRM.Backend.Tests;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public IntegrationTestFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("crm_test")
            .WithUsername("test_user")
            .WithPassword("test_pass")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:Postgres"] = _dbContainer.GetConnectionString(),
                ["Jwt:Key"] = "TestKeyForIntegrationTestsOnly123456789012345678901234567890",
                ["Jwt:Issuer"] = "CRM.Test",
                ["Jwt:Audience"] = "CRM.Test"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // Remover o UserContext real e substituir por um mock
            services.RemoveAll<UserContext>();
            services.AddScoped(_ =>
            {
                return new UserContext
                {
                    UserId = Guid.NewGuid(),
                    Email = "test@example.com",
                    Name = "Test User",
                    CorrelationId = Guid.NewGuid().ToString()
                };
            });

            // Mock do serviço ViaCep para não fazer chamadas HTTP reais
            services.RemoveAll<IViaCepService>();
            var viaCepMock = new Mock<IViaCepService>();
            viaCepMock
                .Setup(x => x.GetAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Application.DTOs.ViaCepResponse
                {
                    Cep = "01001-000",
                    Logradouro = "Praça da Sé",
                    Complemento = "lado ímpar",
                    Bairro = "Sé",
                    Localidade = "São Paulo",
                    Uf = "SP",
                    Erro = false
                });
            services.AddSingleton(viaCepMock.Object);
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Criar as tabelas necessárias
        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS event_store (
                event_id UUID PRIMARY KEY,
                aggregate_id UUID NOT NULL,
                event_type VARCHAR(255) NOT NULL,
                event_data JSONB NOT NULL,
                metadata JSONB,
                version INT NOT NULL,
                occurred_at TIMESTAMP NOT NULL,
                created_at TIMESTAMP DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS idx_event_store_aggregate ON event_store(aggregate_id);

            CREATE TABLE IF NOT EXISTS customers (
                id UUID PRIMARY KEY,
                customer_type VARCHAR(10) NOT NULL,
                name VARCHAR(255) NOT NULL,
                document VARCHAR(14) UNIQUE NOT NULL,
                email VARCHAR(255) UNIQUE NOT NULL,
                birth_date DATE,
                company_name VARCHAR(255),
                state_registration VARCHAR(50),
                address JSONB,
                status VARCHAR(20) NOT NULL,
                created_at TIMESTAMP NOT NULL,
                updated_at TIMESTAMP,
                version INT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_customers_document ON customers(document);
            CREATE INDEX IF NOT EXISTS idx_customers_email ON customers(email);
        ";

        await cmd.ExecuteNonQueryAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}

[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFactory>
{
}