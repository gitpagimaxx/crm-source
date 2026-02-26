using CRM.Backend.Domain.Enums;
using CRM.Backend.Domain.Events;
using CRM.Backend.Domain.Exceptions;
using CRM.Backend.Domain.ValueObjects;

namespace CRM.Backend.Domain.Aggregates;

public class Customer
{
    public Guid Id { get; private set; }
    public CustomerType CustomerType { get; private set; }
    public string Name { get; private set; } = default!;
    public Document Document { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public DateTime? BirthDate { get; private set; }
    public string? CompanyName { get; private set; }
    public string? StateRegistration { get; private set; }
    public Address? Address { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int Version { get; private set; }

    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Customer() { }

    public static Customer Create(
        Guid id,
        CustomerType customerType,
        string name,
        string document,
        string email,
        DateTime? birthDate,
        string? companyName,
        string? stateRegistration,
        Address? address)
    {
        var customer = new Customer();

        if (customerType == CustomerType.PF)
        {
            if (!birthDate.HasValue)
                throw new DomainException("Data de nascimento é obrigatória para PF.");
            var age = CalculateAge(birthDate.Value);
            if (age < 18)
                throw new DomainException("Cliente PF deve ter pelo menos 18 anos.");
        }

        if (customerType == CustomerType.PJ)
        {
            if (string.IsNullOrWhiteSpace(stateRegistration))
                throw new DomainException("Inscrição estadual é obrigatória para PJ (use 'Isento' se não houver).");
        }

        var doc = new Document(document, customerType);

        var evt = new CustomerCreatedEvent
        {
            CustomerId = id,
            CustomerType = customerType,
            Name = name,
            Document = doc.Value,
            Email = email,
            BirthDate = birthDate,
            CompanyName = companyName,
            StateRegistration = stateRegistration,
            Address = address
        };

        customer.Apply(evt);
        customer._domainEvents.Add(evt);
        return customer;
    }

    public void Update(string name, string email, string? companyName, string? stateRegistration, Address? address)
    {
        if (Status == CustomerStatus.Inactive)
            throw new DomainException("Não é possível atualizar um cliente inativo.");

        var evt = new CustomerUpdatedEvent
        {
            CustomerId = Id,
            Name = name,
            Email = email,
            CompanyName = companyName,
            StateRegistration = stateRegistration,
            Address = address
        };

        Apply(evt);
        _domainEvents.Add(evt);
    }

    public void Deactivate(string reason)
    {
        if (Status == CustomerStatus.Inactive)
            throw new DomainException("Cliente já está inativo.");

        var evt = new CustomerDeactivatedEvent { CustomerId = Id, Reason = reason };
        Apply(evt);
        _domainEvents.Add(evt);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void Apply(CustomerCreatedEvent evt)
    {
        Id = evt.CustomerId;
        CustomerType = evt.CustomerType;
        Name = evt.Name;
        Document = new Document(evt.Document, evt.CustomerType);
        Email = evt.Email;
        BirthDate = evt.BirthDate;
        CompanyName = evt.CompanyName;
        StateRegistration = evt.StateRegistration;
        Address = evt.Address;
        Status = CustomerStatus.Active;
        CreatedAt = evt.OccurredAt;
        Version = 1;
    }

    private void Apply(CustomerUpdatedEvent evt)
    {
        Name = evt.Name;
        Email = evt.Email;
        CompanyName = evt.CompanyName;
        StateRegistration = evt.StateRegistration;
        Address = evt.Address;
        UpdatedAt = evt.OccurredAt;
        Version++;
    }

    private void Apply(CustomerDeactivatedEvent evt)
    {
        Status = CustomerStatus.Inactive;
        UpdatedAt = evt.OccurredAt;
        Version++;
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.AddYears(age) > today) age--;
        return age;
    }

    public static Customer Reconstitute(IEnumerable<DomainEvent> events)
    {
        var customer = new Customer();
        foreach (var evt in events)
        {
            switch (evt)
            {
                case CustomerCreatedEvent e: customer.Apply(e); break;
                case CustomerUpdatedEvent e: customer.Apply(e); break;
                case CustomerDeactivatedEvent e: customer.Apply(e); break;
            }
        }
        return customer;
    }
}