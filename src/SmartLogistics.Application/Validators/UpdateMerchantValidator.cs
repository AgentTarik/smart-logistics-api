using FluentValidation;
using SmartLogistics.Application.Features.Merchants.Commands;

namespace SmartLogistics.Application.Validators;

public class UpdateMerchantValidator : AbstractValidator<UpdateMerchantCommand>
{
    public UpdateMerchantValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Request.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Request.Longitude).InclusiveBetween(-180, 180);
    }
}
