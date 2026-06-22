using FluentValidation;
using SunAdmin.Contracts.Settings;

namespace SunAdmin.Api.Validation;

public sealed class UpdateSettingRequestValidator : AbstractValidator<UpdateSettingRequest>
{
    public UpdateSettingRequestValidator()
    {
        RuleFor(x => x.Value).NotEmpty().MaximumLength(512);
    }
}
