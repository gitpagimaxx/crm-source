using CRM.Backend.Domain.Aggregates;
using CRM.Backend.Domain.Enums;
using CRM.Backend.Domain.Events;
using CRM.Backend.Domain.Exceptions;
using CRM.Backend.Domain.ValueObjects;
using FluentAssertions;

namespace CRM.Backend.Tests.Unit;

public class CustomerTests
{
    [Fact]
    public void Create_ValidPF_ShouldCreateCustomerSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "João Silva";
        var cpf = "12345678909";
        var email = "joao@example.com";
        var birthDate = DateTime.Now.AddYears(-25);
        var address = new Address("12345-678", "Rua Teste", "100", null, "Centro", "São Paulo", "SP");

        // Act
        var customer = Customer.Create(
            id,
            CustomerType.PF,
            name,
            cpf,
            email,
            birthDate,
            null,
            null,
            address);

        // Assert
        customer.Should().NotBeNull();
        customer.Id.Should().Be(id);
        customer.CustomerType.Should().Be(CustomerType.PF);
        customer.Name.Should().Be(name);
        customer.Email.Should().Be(email);
        customer.BirthDate.Should().Be(birthDate);
        customer.Address.Should().Be(address);
        customer.Status.Should().Be(CustomerStatus.Active);
        customer.Version.Should().Be(1);
        customer.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CustomerCreatedEvent>();
    }

    [Fact]
    public void Create_ValidPJ_ShouldCreateCustomerSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Empresa LTDA";
        var cnpj = "11222333000181";
        var email = "contato@empresa.com";
        var companyName = "Empresa LTDA";
        var stateRegistration = "123456789";

        // Act
        var customer = Customer.Create(
            id,
            CustomerType.PJ,
            name,
            cnpj,
            email,
            null,
            companyName,
            stateRegistration,
            null);

        // Assert
        customer.Should().NotBeNull();
        customer.Id.Should().Be(id);
        customer.CustomerType.Should().Be(CustomerType.PJ);
        customer.CompanyName.Should().Be(companyName);
        customer.StateRegistration.Should().Be(stateRegistration);
        customer.Status.Should().Be(CustomerStatus.Active);
    }

    [Fact]
    public void Create_PFWithoutBirthDate_ShouldThrowDomainException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Action act = () => Customer.Create(
            id,
            CustomerType.PF,
            "João Silva",
            "12345678909",
            "joao@example.com",
            null,
            null,
            null,
            null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Data de nascimento é obrigatória para PF.");
    }

    [Fact]
    public void Create_PFUnder18_ShouldThrowDomainException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var birthDate = DateTime.Now.AddYears(-17);

        // Act
        Action act = () => Customer.Create(
            id,
            CustomerType.PF,
            "João Silva",
            "12345678909",
            "joao@example.com",
            birthDate,
            null,
            null,
            null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Cliente PF deve ter pelo menos 18 anos.");
    }

    [Fact]
    public void Create_PJWithoutStateRegistration_ShouldThrowDomainException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Action act = () => Customer.Create(
            id,
            CustomerType.PJ,
            "Empresa LTDA",
            "11222333000181",
            "contato@empresa.com",
            null,
            "Empresa LTDA",
            null,
            null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Inscrição estadual é obrigatória para PJ (use 'Isento' se não houver).");
    }

    [Fact]
    public void Update_ActiveCustomer_ShouldUpdateSuccessfully()
    {
        // Arrange
        var customer = CreateValidPFCustomer();
        var newName = "João Santos";
        var newEmail = "joao.santos@example.com";

        // Act
        customer.Update(newName, newEmail, null, null, null);

        // Assert
        customer.Name.Should().Be(newName);
        customer.Email.Should().Be(newEmail);
        customer.Version.Should().Be(2);
        customer.UpdatedAt.Should().NotBeNull();
        customer.DomainEvents.Should().HaveCount(2);
        customer.DomainEvents.Last().Should().BeOfType<CustomerUpdatedEvent>();
    }

    [Fact]
    public void Update_InactiveCustomer_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateValidPFCustomer();
        customer.Deactivate("Teste");

        // Act
        Action act = () => customer.Update("Novo Nome", "novo@email.com", null, null, null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Não é possível atualizar um cliente inativo.");
    }

    [Fact]
    public void Deactivate_ActiveCustomer_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var customer = CreateValidPFCustomer();
        var reason = "Cliente solicitou";

        // Act
        customer.Deactivate(reason);

        // Assert
        customer.Status.Should().Be(CustomerStatus.Inactive);
        customer.Version.Should().Be(2);
        customer.UpdatedAt.Should().NotBeNull();
        customer.DomainEvents.Should().HaveCount(2);
        customer.DomainEvents.Last().Should().BeOfType<CustomerDeactivatedEvent>()
            .Which.Reason.Should().Be(reason);
    }

    [Fact]
    public void Deactivate_InactiveCustomer_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateValidPFCustomer();
        customer.Deactivate("Teste");

        // Act
        Action act = () => customer.Deactivate("Outra razão");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Cliente já está inativo.");
    }

    [Fact]
    public void Reconstitute_FromEvents_ShouldRebuildCustomer()
    {
        // Arrange
        var id = Guid.NewGuid();
        var events = new List<DomainEvent>
        {
            new CustomerCreatedEvent
            {
                CustomerId = id,
                CustomerType = CustomerType.PF,
                Name = "João Silva",
                Document = "12345678909",
                Email = "joao@example.com",
                BirthDate = DateTime.Now.AddYears(-25)
            },
            new CustomerUpdatedEvent
            {
                CustomerId = id,
                Name = "João Santos",
                Email = "joao.santos@example.com"
            }
        };

        // Act
        var customer = Customer.Reconstitute(events);

        // Assert
        customer.Should().NotBeNull();
        customer.Id.Should().Be(id);
        customer.Name.Should().Be("João Santos");
        customer.Email.Should().Be("joao.santos@example.com");
        customer.Version.Should().Be(2);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var customer = CreateValidPFCustomer();

        // Act
        customer.ClearDomainEvents();

        // Assert
        customer.DomainEvents.Should().BeEmpty();
    }

    private static Customer CreateValidPFCustomer()
    {
        return Customer.Create(
            Guid.NewGuid(),
            CustomerType.PF,
            "João Silva",
            "12345678909",
            "joao@example.com",
            DateTime.Now.AddYears(-25),
            null,
            null,
            null);
    }
}