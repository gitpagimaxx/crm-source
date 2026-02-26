using CRM.Backend.Domain.Interfaces;
using CRM.Backend.Domain.Model;
using Dapper;

namespace CRM.Backend.Infra.Persistence;

public class CustomerReadRepository(DbConnectionFactory factory) : ICustomerReadRepository
{
    private readonly DbConnectionFactory _factory = factory;

    public async Task<CustomerReadModel?> GetById(Guid id, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync<CustomerRow>(
            "SELECT * FROM customers_read WHERE id = @id", new { id });
        return row is null ? null : MapToModel(row);
    }

    public async Task<IEnumerable<CustomerReadModel>> GetAll(int page, int pageSize, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var rows = await conn.QueryAsync<CustomerRow>(
            "SELECT * FROM customers_read ORDER BY created_at DESC LIMIT @pageSize OFFSET @offset",
            new { pageSize, offset = (page - 1) * pageSize });
        return rows.Select(MapToModel);
    }

    public async Task<bool> ExistsByDocument(string document, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM customers_read WHERE document = @document", new { document });
        return count > 0;
    }

    public async Task<bool> ExistsByEmail(string email, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM customers_read WHERE email = @email", new { email });
        return count > 0;
    }

    public async Task<bool> ExistsByDocumentExcludingId(string document, Guid excludeId, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM customers_read WHERE document = @document AND id != @excludeId",
            new { document, excludeId });
        return count > 0;
    }

    public async Task<bool> ExistsByEmailExcludingId(string email, Guid excludeId, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM customers_read WHERE email = @email AND id != @excludeId",
            new { email, excludeId });
        return count > 0;
    }

    public async Task Upsert(CustomerReadModel model, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO customers_read (id, customer_type, name, document, email, birth_date, company_name, state_registration,
                zip_code, street, number, complement, neighborhood, city, state, status, created_at, updated_at)
            VALUES (@Id, @CustomerType, @Name, @Document, @Email, @BirthDate, @CompanyName, @StateRegistration,
                @ZipCode, @Street, @Number, @Complement, @Neighborhood, @City, @State, @Status, @CreatedAt, @UpdatedAt)
            ON CONFLICT(id) DO UPDATE SET
                customer_type = @CustomerType,
                name = @Name,
                document = @Document,
                email = @Email,
                birth_date = @BirthDate,
                company_name = @CompanyName,
                state_registration = @StateRegistration,
                zip_code = @ZipCode,
                street = @Street,
                number = @Number,
                complement = @Complement,
                neighborhood = @Neighborhood,
                city = @City,
                state = @State,
                status = @Status,
                updated_at = @UpdatedAt
        ", model);
    }

    private static CustomerReadModel MapToModel(CustomerRow row) =>
        new(
            row.Id,
            row.CustomerType,
            row.Name,
            row.Document,
            row.Email,
            row.BirthDate,
            row.CompanyName,
            row.StateRegistration,
            row.ZipCode,
            row.Street,
            row.Number,
            row.Complement,
            row.Neighborhood,
            row.City,
            row.State,
            row.Status,
            row.CreatedAt,
            row.UpdatedAt
        );

    private record CustomerRow(
        Guid Id,
        string CustomerType,
        string Name,
        string Document,
        string Email,
        DateTime? BirthDate,
        string? CompanyName,
        string? StateRegistration,
        string? ZipCode,
        string? Street,
        string? Number,
        string? Complement,
        string? Neighborhood,
        string? City,
        string? State,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}