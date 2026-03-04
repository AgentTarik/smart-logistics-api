using FluentValidation;
using SmartLogistics.Application.Features.Drivers.Commands;

namespace SmartLogistics.Application.Validators;

public class CreateDriverValidator : AbstractValidator<CreateDriverCommand>
{
    public CreateDriverValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.PhoneNumber).NotEmpty();
        RuleFor(x => x.Request.LicensePlate).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.MaxCargoWeightKg).GreaterThan(0);
        RuleFor(x => x.Request.MaxCargoVolumeM3).GreaterThan(0);
    }
}
