using CRM.Backend.Domain.Model;

namespace CRM.Backend.Domain.Interfaces;

public interface ICustomerReadRepository
{
    Task<CustomerReadModel?> GetById(Guid id, CancellationToken ct = default);
    Task<IEnumerable<CustomerReadModel>> GetAll(int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsByDocument(string document, CancellationToken ct = default);
    Task<bool> ExistsByEmail(string email, CancellationToken ct = default);
    Task<bool> ExistsByDocumentExcludingId(string document, Guid excludeId, CancellationToken ct = default);
    Task<bool> ExistsByEmailExcludingId(string email, Guid excludeId, CancellationToken ct = default);
    Task Upsert(CustomerReadModel model, CancellationToken ct = default);
}