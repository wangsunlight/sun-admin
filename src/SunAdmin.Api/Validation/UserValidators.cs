using FluentValidation;
using SunAdmin.Contracts.Users;

namespace SunAdmin.Api.Validation;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(64);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(128);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(128);
    }
}

public sealed class BatchUserRequestValidator : AbstractValidator<BatchUserRequest>
{
    public BatchUserRequestValidator()
    {
        RuleFor(x => x.UserIds).NotEmpty();
        RuleForEach(x => x.UserIds).GreaterThan(0);
    }
}
