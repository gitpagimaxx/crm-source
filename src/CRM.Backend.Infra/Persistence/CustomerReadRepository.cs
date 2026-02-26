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

    public async Task Upsert(CustomerReadModel model, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO customers_read (id, customer_type, name, document, email, birth_date, company_name, state_registration,
                zip_code, street, number, complement, neighborhood, city, state, status, created_at, updated_at)
            VALUES (@Id, @CustomerType, @Name, @Document, @Email, @BirthDate, @CompanyName, @StateRegistration,
                @ZipCode, @Street, @Number, @Complement, @Neighborhood, @City, @State, @Status, @CreatedAt, @UpdatedAt)
            ON CONFLICT (id) DO UPDATE SET
                name = EXCLUDED.name, email = EXCLUDED.email, company_name = EXCLUDED.company_name,
                state_registration = EXCLUDED.state_registration, zip_code = EXCLUDED.zip_code,
                street = EXCLUDED.street, number = EXCLUDED.number, complement = EXCLUDED.complement,
                neighborhood = EXCLUDED.neighborhood, city = EXCLUDED.city, state = EXCLUDED.state,
                status = EXCLUDED.status, updated_at = EXCLUDED.updated_at", model);
    }

    private static CustomerReadModel MapToModel(CustomerRow r) => new(
        r.id, r.customer_type, r.name, r.document, r.email,
        r.birth_date.HasValue ? DateOnly.FromDateTime(r.birth_date.Value) : null,
        r.company_name, r.state_registration,
        r.zip_code, r.street, r.number, r.complement, r.neighborhood, r.city, r.state,
        r.status, r.created_at, r.updated_at
    );

    private record CustomerRow(Guid id, string customer_type, string name, string document, string email,
        DateTime? birth_date, string? company_name, string? state_registration,
        string? zip_code, string? street, string? number, string? complement,
        string? neighborhood, string? city, string? state,
        string status, DateTime created_at, DateTime? updated_at);
}