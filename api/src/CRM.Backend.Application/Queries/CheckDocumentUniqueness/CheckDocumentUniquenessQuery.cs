using MediatR;

namespace CRM.Backend.Application.Queries.CheckDocumentUniqueness;

public record CheckDocumentUniquenessQuery(string Document) : IRequest<CheckDocumentUniquenessResult>;

public record CheckDocumentUniquenessResult(bool IsAvailable, string CleanedDocument);