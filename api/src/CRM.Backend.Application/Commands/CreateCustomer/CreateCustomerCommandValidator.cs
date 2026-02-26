using FluentValidation;

namespace CRM.Backend.Application.Commands.CreateCustomer;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerType)
            .NotEmpty()
            .Must(t => t == "PF" || t == "PJ")
            .WithMessage("CustomerType must be PF or PJ.");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Document).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);

        When(x => x.CustomerType == "PF", () =>
        {
            RuleFor(x => x.BirthDate).NotNull().WithMessage("BirthDate is required for PF.");
        });

        When(x => x.CustomerType == "PJ", () =>
        {
            RuleFor(x => x.CompanyName).NotEmpty().WithMessage("CompanyName is required for PJ.");
            RuleFor(x => x.StateRegistration).NotEmpty().WithMessage("StateRegistration is required for PJ.");
        });
    }
}