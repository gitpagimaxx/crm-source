using CRM.Backend.Domain.Interfaces;
using MediatR;

namespace CRM.Backend.Application.Queries.CheckDocumentUniqueness;

public class CheckDocumentUniquenessQueryHandler(ICustomerReadRepository readRepo) 
    : IRequestHandler<CheckDocumentUniquenessQuery, CheckDocumentUniquenessResult>
{
    private readonly ICustomerReadRepository _readRepo = readRepo;

    public async Task<CheckDocumentUniquenessResult> Handle(CheckDocumentUniquenessQuery request, CancellationToken cancellationToken)
    {
        var cleanedDoc = new string([.. request.Document.Where(char.IsDigit)]);
        var exists = await _readRepo.ExistsByDocument(cleanedDoc, cancellationToken);
        
        return new CheckDocumentUniquenessResult(!exists, cleanedDoc);
    }
}