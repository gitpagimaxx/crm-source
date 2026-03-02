using CRM.Backend.Application.Commands.CreateCustomer;
using CRM.Backend.Domain.Exceptions;
using CRM.Backend.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CRM.Backend.Tests.Integration;

[Collection("Integration Tests")]
public class CreateCustomerIntegrationTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public CreateCustomerIntegrationTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateCustomer_CompleteFlow_ShouldPersistAndRetrieve()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStoreRepository>();
        var readRepo = scope.ServiceProvider.GetRequiredService<ICustomerReadRepository>();

        var uniqueCpf = GenerateValidCpf();
        var command = new CreateCustomerCommand(
            "PF",
            "João da Silva Integration Test",
            uniqueCpf,
            $"joao.integration.{Guid.NewGuid()}@example.com",
            DateTime.Now.AddYears(-30),
            null, 
            null,
            "01001-000",
            "100",
            "Apto 10"
        );

        // Act
        var customerId = await mediator.Send(command);

        // Assert
        customerId.Should().NotBeEmpty();

        await Task.Delay(500);

        var cleanedCpf = new string([.. uniqueCpf.Where(char.IsDigit)]);
        var customerExists = await readRepo.ExistsByDocument(cleanedCpf, CancellationToken.None);
        customerExists.Should().BeTrue();

        var emailExists = await readRepo.ExistsByEmail(command.Email, CancellationToken.None);
        emailExists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCustomer_DuplicateDocument_ShouldThrowException()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var document = GenerateValidCpf();
        var command1 = new CreateCustomerCommand(
            "PF",
            "Primeiro Cliente",
            document,
            $"primeiro.{Guid.NewGuid()}@example.com",
            DateTime.Now.AddYears(-25),
            null,
            null, 
            null,  
            null,  
            null   
        );

        await mediator.Send(command1);

        var command2 = new CreateCustomerCommand(
            "PF",
            "Segundo Cliente",
            document,
            $"segundo.{Guid.NewGuid()}@example.com",
            DateTime.Now.AddYears(-30),
            null, 
            null,  
            null,  
            null,  
            null   
        );

        // Act & Assert
        await Assert.ThrowsAsync<Domain.Exceptions.DomainException>(
            async () => await mediator.Send(command2));
    }

    [Fact]
    public async Task CreateCustomer_PJ_WithStateRegistration_ShouldSucceed()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var uniqueCnpj = GenerateValidCnpj();
        var command = new CreateCustomerCommand(
            "PJ",
            "Empresa Teste LTDA",
            uniqueCnpj,
            $"empresa.{Guid.NewGuid()}@example.com",
            null,
            "Empresa Teste LTDA",
            "123456789",
            null,
            null, 
            null
        );

        // Act
        var customerId = await mediator.Send(command);

        // Assert
        customerId.Should().NotBeEmpty();
    }

    // Métodos auxiliares para gerar CPF/CNPJ válidos
    private static string GenerateValidCpf()
    {
        var random = new Random();
        var cpf = new int[11];

        for (int i = 0; i < 9; i++)
        {
            cpf[i] = random.Next(0, 10);
        }

        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += cpf[i] * (10 - i);
        }
        int remainder = sum % 11;
        cpf[9] = remainder < 2 ? 0 : 11 - remainder;

        sum = 0;
        for (int i = 0; i < 10; i++)
        {
            sum += cpf[i] * (11 - i);
        }
        remainder = sum % 11;
        cpf[10] = remainder < 2 ? 0 : 11 - remainder;

        return string.Join("", cpf);
    }

    private static string GenerateValidCnpj()
    {
        var random = new Random();
        var cnpj = new int[14];

        for (int i = 0; i < 12; i++)
        {
            cnpj[i] = random.Next(0, 10);
        }

        int[] multiplier1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            sum += cnpj[i] * multiplier1[i];
        }
        int remainder = sum % 11;
        cnpj[12] = remainder < 2 ? 0 : 11 - remainder;

        int[] multiplier2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        sum = 0;
        for (int i = 0; i < 13; i++)
        {
            sum += cnpj[i] * multiplier2[i];
        }
        remainder = sum % 11;
        cnpj[13] = remainder < 2 ? 0 : 11 - remainder;

        return string.Join("", cnpj);
    }
}