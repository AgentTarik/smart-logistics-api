using FluentValidation;
using SmartLogistics.Application.Features.Zones.Commands;

namespace SmartLogistics.Application.Validators;

public class UpdateZoneValidator : AbstractValidator<UpdateZoneCommand>
{
    public UpdateZoneValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.BaseDeliveryCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.BoundaryCoordinates)
            .Must(c => c == null || c.Length >= 3)
            .WithMessage("A polygon must have at least 3 points");
    }
}
